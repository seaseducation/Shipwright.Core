// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar;
using Lamar.Scanning.Conventions;
using Moq;
using Shipwright.Databases.Internal;
using Shipwright.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Databases
{
    public class LamarDatabaseExtensionsTests
    {
        private ServiceRegistry registry = new ServiceRegistry();

        public class AddDbConnectionDispatch : LamarDatabaseExtensionsTests
        {
            private ServiceRegistry method() => registry.AddDbConnectionDispatch();

            [Fact]
            public void requires_registry()
            {
                registry = null!;
                Assert.Throws<ArgumentNullException>( nameof( registry ), method );
            }

            [Fact]
            public void registers_dispatcher()
            {
                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( actual );
                var instance = container.GetInstance<IDbConnectionDispatcher>();
                Assert.IsType<DbConnectionDispatcher>( instance );
            }
        }

        public class AddDbConnectionBuilders : LamarDatabaseExtensionsTests
        {
            private IAssemblyScanner scanner;
            private IAssemblyScanner method() => scanner.AddDbConnectionBuilders();

            public class FakeConnectionBuilder : IDbConnectionBuilder<FakeConnectionInfo>
            {
                public Task<IDbConnectionFactory> Build( FakeConnectionInfo connectionInfo, CancellationToken cancellationToken ) =>
                    throw new NotImplementedException();
            }

            [Fact]
            public void requires_scanner()
            {
                scanner = null!;
                Assert.Throws<ArgumentNullException>( nameof( scanner ), method );
            }

            [Fact]
            public void registers_discovered_builders()
            {
                registry.Scan( scanner =>
                {
                    this.scanner = scanner;
                    scanner.AssemblyContainingType<DbConnectionInfo>(); // sanity check to ensure decorators aren't grabbed
                    scanner.AssemblyContainingType<AddDbConnectionBuilders>();

                    var actual = method();
                    Assert.Same( this.scanner, actual );
                } );

                using var container = new Container( registry );
                var instance = container.GetInstance<IDbConnectionBuilder<FakeConnectionInfo>>();
                Assert.IsType<FakeConnectionBuilder>( instance );
            }
        }

        public class AddDbConnectionValidation : LamarDatabaseExtensionsTests
        {
            private ServiceRegistry method() => registry.AddDbConnectionValidation();

            [Fact]
            public void requires_registry()
            {
                registry = null!;
                Assert.Throws<ArgumentNullException>( nameof( registry ), method );
            }

            [Fact]
            public void decorates_handler()
            {
                var inner = Mockery.Of<IDbConnectionBuilder<FakeConnectionInfo>>();
                var validator = Mockery.Of<IValidationAdapter<FakeConnectionInfo>>();
                registry.For<IDbConnectionBuilder<FakeConnectionInfo>>().Add( inner );
                registry.For<IValidationAdapter<FakeConnectionInfo>>().Add( validator );

                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( registry );
                var instance = container.GetInstance<IDbConnectionBuilder<FakeConnectionInfo>>();
                var decorator = Assert.IsType<ValidationDecorator<FakeConnectionInfo>>( instance );
                Assert.Same( inner, decorator.inner );
                Assert.Same( validator, decorator.validator );
            }
        }

        public class AddDbConnectionCancellation : LamarDatabaseExtensionsTests
        {
            private ServiceRegistry method() => registry.AddDbConnectionCancellation();

            [Fact]
            public void requires_registry()
            {
                registry = null!;
                Assert.Throws<ArgumentNullException>( nameof( registry ), method );
            }

            [Fact]
            public void decorates_handler()
            {
                var inner = Mockery.Of<IDbConnectionBuilder<FakeConnectionInfo>>();
                registry.For<IDbConnectionBuilder<FakeConnectionInfo>>().Add( inner );

                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( registry );
                var instance = container.GetInstance<IDbConnectionBuilder<FakeConnectionInfo>>();
                var decorator = Assert.IsType<CancellationDecorator<FakeConnectionInfo>>( instance );
                Assert.Same( inner, decorator.inner );
            }
        }
    }
}
