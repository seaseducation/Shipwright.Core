// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar;
using Lamar.Scanning.Conventions;
using Moq;
using Shipwright.Dataflows.Transformations;
using Shipwright.Dataflows.Transformations.Internal;
using Shipwright.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Dataflows
{
    public class LamarDataflowTransformationExtensionsTests
    {
        private ServiceRegistry registry = new ServiceRegistry();

        public class AddTransformationDispatch : LamarDataflowTransformationExtensionsTests
        {
            private ServiceRegistry method() => registry.AddTransformationDispatch();

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
                var instance = container.GetInstance<ITransformationDispatcher>();
                Assert.IsType<TransformationDispatcher>( instance );
            }
        }

        public class AddTransformationFactories : LamarDataflowTransformationExtensionsTests
        {
            private IAssemblyScanner scanner;
            private IAssemblyScanner method() => scanner.AddTransformationFactories();

            public class FakeTransformationFactory : TransformationFactory<FakeTransformation>
            {
                protected override Task<ITransformationHandler> Create( FakeTransformation transformation, CancellationToken cancellationToken ) =>
                    throw new NotImplementedException();
            }

            [Fact]
            public void requires_scanner()
            {
                scanner = null!;
                Assert.Throws<ArgumentNullException>( nameof( scanner ), method );
            }

            [Fact]
            public void registers_discovered_factories()
            {
                registry.Scan( scanner =>
                {
                    this.scanner = scanner;
                    scanner.AssemblyContainingType<Transformation>(); // sanity check to ensure decorators aren't grabbed
                    scanner.AssemblyContainingType<AddTransformationFactories>();

                    var actual = method();
                    Assert.Same( this.scanner, actual );
                } );

                using var container = new Container( registry );
                var instance = container.GetInstance<ITransformationFactory<FakeTransformation>>();
                Assert.IsType<FakeTransformationFactory>( instance );
            }
        }

        public class AddTransformationValidation : LamarDataflowTransformationExtensionsTests
        {
            private ServiceRegistry method() => registry.AddTransformationValidation();

            [Fact]
            public void requires_registry()
            {
                registry = null!;
                Assert.Throws<ArgumentNullException>( nameof( registry ), method );
            }

            [Fact]
            public void decorates_handler()
            {
                var inner = Mockery.Of<ITransformationFactory<FakeTransformation>>();
                var validator = Mockery.Of<IValidationAdapter<FakeTransformation>>();
                registry.For<ITransformationFactory<FakeTransformation>>().Add( inner );
                registry.For<IValidationAdapter<FakeTransformation>>().Add( validator );

                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( registry );
                var instance = container.GetInstance<ITransformationFactory<FakeTransformation>>();
                var decorator = Assert.IsType<ValidationFactoryDecorator<FakeTransformation>>( instance );
                Assert.Same( inner, decorator.inner );
                Assert.Same( validator, decorator.validator );
            }
        }

        public class AddTransformationCancellation : LamarDataflowTransformationExtensionsTests
        {
            private ServiceRegistry method() => registry.AddTransformationCancellation();

            [Fact]
            public void requires_registry()
            {
                registry = null!;
                Assert.Throws<ArgumentNullException>( nameof( registry ), method );
            }

            [Fact]
            public void decorates_handler()
            {
                var inner = Mockery.Of<ITransformationFactory<FakeTransformation>>();
                registry.For<ITransformationFactory<FakeTransformation>>().Add( inner );

                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( registry );
                var instance = container.GetInstance<ITransformationFactory<FakeTransformation>>();
                var decorator = Assert.IsType<CancellationFactoryDecorator<FakeTransformation>>( instance );
                Assert.Same( inner, decorator.inner );
            }
        }
    }
}
