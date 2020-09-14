// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar;
using Lamar.Scanning.Conventions;
using Moq;
using Shipwright.Commands.Internal;
using Shipwright.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Commands
{
    public class LamarCommandExtensionsTests
    {
        private ServiceRegistry registry = new ServiceRegistry();

        public class AddCommandDispatch : LamarCommandExtensionsTests
        {
            private ServiceRegistry method() => registry.AddCommandDispatch();

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
                var instance = container.GetInstance<ICommandDispatcher>();
                Assert.IsType<CommandDispatcher>( instance );
            }
        }

        public class AddCommandHandlers : LamarCommandExtensionsTests
        {
            private IAssemblyScanner scanner;
            private IAssemblyScanner method() => scanner.AddCommandHandlers();

            public class FakeCommandHandler : CommandHandler<FakeGuidCommand, Guid>
            {
                protected override Task<Guid> Execute( FakeGuidCommand command, CancellationToken cancellationToken ) =>
                    throw new NotImplementedException();
            }

            [Fact]
            public void requires_scanner()
            {
                scanner = null!;
                Assert.Throws<ArgumentNullException>( nameof( scanner ), method );
            }

            [Fact]
            public void registers_discovered_command_handlers()
            {
                registry.Scan( scanner =>
                {
                    this.scanner = scanner;
                    scanner.AssemblyContainingType<Command>(); // sanity check to ensure decorators aren't grabbed
                    scanner.AssemblyContainingType<AddCommandHandlers>();

                    var actual = method();
                    Assert.Same( this.scanner, actual );
                } );

                using var container = new Container( registry );
                var instance = container.GetInstance<ICommandHandler<FakeGuidCommand, Guid>>();
                Assert.IsType<FakeCommandHandler>( instance );
            }
        }

        public class AddCommandValidation : LamarCommandExtensionsTests
        {
            private ServiceRegistry method() => registry.AddCommandValidation();

            [Fact]
            public void requires_registry()
            {
                registry = null!;
                Assert.Throws<ArgumentNullException>( nameof( registry ), method );
            }

            [Fact]
            public void decorates_handler()
            {
                var inner = Mockery.Of<ICommandHandler<FakeGuidCommand, Guid>>();
                var validator = Mockery.Of<IValidationAdapter<FakeGuidCommand>>();
                registry.For<ICommandHandler<FakeGuidCommand, Guid>>().Add( inner );
                registry.For<IValidationAdapter<FakeGuidCommand>>().Add( validator );

                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( registry );
                var instance = container.GetInstance<ICommandHandler<FakeGuidCommand, Guid>>();
                var decorator = Assert.IsType<ValidationDecorator<FakeGuidCommand, Guid>>( instance );
                Assert.Same( inner, decorator.inner );
                Assert.Same( validator, decorator.validator );
            }
        }

        public class AddCommandCancellation : LamarCommandExtensionsTests
        {
            private ServiceRegistry method() => registry.AddCommandCancellation();

            [Fact]
            public void requires_registry()
            {
                registry = null!;
                Assert.Throws<ArgumentNullException>( nameof( registry ), method );
            }

            [Fact]
            public void decorates_handler()
            {
                var inner = Mockery.Of<ICommandHandler<FakeGuidCommand, Guid>>();
                registry.For<ICommandHandler<FakeGuidCommand, Guid>>().Add( inner );

                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( registry );
                var instance = container.GetInstance<ICommandHandler<FakeGuidCommand, Guid>>();
                var decorator = Assert.IsType<CancellationDecorator<FakeGuidCommand, Guid>>( instance );
                Assert.Same( inner, decorator.inner );
            }
        }
    }
}
