// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar;
using Lamar.Scanning.Conventions;
using Moq;
using Shipwright.Dataflows.Sources;
using Shipwright.Dataflows.Sources.Internal;
using Shipwright.Validation;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Shipwright.Dataflows
{
    public class LamarDataflowSourceExtensionsTests
    {
        private ServiceRegistry registry = new ServiceRegistry();

        public class AddSourceDispatch : LamarDataflowSourceExtensionsTests
        {
            private ServiceRegistry method() => registry.AddSourceDispatch();

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
                var instance = container.GetInstance<ISourceDispatcher>();
                Assert.IsType<SourceDispatcher>( instance );
            }
        }

        public class AddSourceHandlers : LamarDataflowSourceExtensionsTests
        {
            private IAssemblyScanner scanner;
            private IAssemblyScanner method() => scanner.AddSourceHandlers();

            public class FakeSourceHandler : SourceHandler<FakeSource>
            {
                protected override IAsyncEnumerable<Record> Read( FakeSource source, StringComparer comparer, CancellationToken cancellationToken ) => throw new NotImplementedException();
            }

            [Fact]
            public void requires_scanner()
            {
                scanner = null!;
                Assert.Throws<ArgumentNullException>( nameof( scanner ), method );
            }

            [Fact]
            public void registers_discovered_handlers()
            {
                registry.Scan( scanner =>
                {
                    this.scanner = scanner;
                    scanner.AssemblyContainingType<Source>(); // sanity check to ensure decorators aren't grabbed
                    scanner.AssemblyContainingType<AddSourceHandlers>();

                    var actual = method();
                    Assert.Same( this.scanner, actual );
                } );

                using var container = new Container( registry );
                var instance = container.GetInstance<ISourceHandler<FakeSource>>();
                Assert.IsType<FakeSourceHandler>( instance );
            }
        }

        public class AddSourceValidation : LamarDataflowSourceExtensionsTests
        {
            private ServiceRegistry method() => registry.AddSourceValidation();

            [Fact]
            public void requires_registry()
            {
                registry = null!;
                Assert.Throws<ArgumentNullException>( nameof( registry ), method );
            }

            [Fact]
            public void decorates_handler()
            {
                var inner = Mockery.Of<ISourceHandler<FakeSource>>();
                var validator = Mockery.Of<IValidationAdapter<FakeSource>>();
                registry.For<ISourceHandler<FakeSource>>().Add( inner );
                registry.For<IValidationAdapter<FakeSource>>().Add( validator );

                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( registry );
                var instance = container.GetInstance<ISourceHandler<FakeSource>>();
                var decorator = Assert.IsType<ValidationDecorator<FakeSource>>( instance );
                Assert.Same( inner, decorator.inner );
                Assert.Same( validator, decorator.validator );
            }
        }

        public class AddSourceCancellation : LamarDataflowSourceExtensionsTests
        {
            private ServiceRegistry method() => registry.AddSourceCancellation();

            [Fact]
            public void requires_registry()
            {
                registry = null!;
                Assert.Throws<ArgumentNullException>( nameof( registry ), method );
            }

            [Fact]
            public void decorates_handler()
            {
                var inner = Mockery.Of<ISourceHandler<FakeSource>>();
                registry.For<ISourceHandler<FakeSource>>().Add( inner );

                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( registry );
                var instance = container.GetInstance<ISourceHandler<FakeSource>>();
                var decorator = Assert.IsType<CancellationDecorator<FakeSource>>( instance );
                Assert.Same( inner, decorator.inner );
            }
        }
    }
}
