// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar;
using Lamar.Scanning.Conventions;
using Moq;
using Shipwright.Dataflows.Notifications;
using Shipwright.Dataflows.Sources;
using Shipwright.Dataflows.Transformations;
using Shipwright.Validation;
using System;
using Xunit;

namespace Shipwright.Dataflows
{
    public class LamarDataflowExtensionsTests
    {
        private ServiceRegistry registry = new ServiceRegistry();

        public class AddDataflow : LamarDataflowExtensionsTests
        {
            private ServiceRegistry method() => registry.AddDataflow();

            [Fact]
            public void requires_registry()
            {
                registry = null!;
                Assert.Throws<ArgumentNullException>( nameof( registry ), method );
            }

            [Fact]
            public void registers_source_dispatcher()
            {
                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( actual );
                var instance = container.GetInstance<ISourceDispatcher>();
                Assert.IsType<Sources.Internal.SourceDispatcher>( instance );
            }

            [Fact]
            public void decorates_source_handler()
            {
                var inner = Mockery.Of<ISourceHandler<FakeSource>>();
                var validator = Mockery.Of<IValidationAdapter<FakeSource>>();
                registry.For<ISourceHandler<FakeSource>>().Add( inner );
                registry.For<IValidationAdapter<FakeSource>>().Add( validator );

                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( registry );
                var instance = container.GetInstance<ISourceHandler<FakeSource>>();

                // outer decorator should be cancellation
                var cancellation = Assert.IsType<Sources.Internal.CancellationDecorator<FakeSource>>( instance );

                // inner decorator should be validation
                var validation = Assert.IsType<Sources.Internal.ValidationDecorator<FakeSource>>( cancellation.inner );

                Assert.Same( inner, validation.inner );
                Assert.Same( validator, validation.validator );
            }

            [Fact]
            public void registers_transformation_dispatcher()
            {
                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( actual );
                var instance = container.GetInstance<ITransformationDispatcher>();
                Assert.IsType<Transformations.Internal.TransformationDispatcher>( instance );
            }

            [Fact]
            public void decorates_transformation_handler()
            {
                var inner = Mockery.Of<ITransformationFactory<FakeTransformation>>();
                var validator = Mockery.Of<IValidationAdapter<FakeTransformation>>();
                registry.For<ITransformationFactory<FakeTransformation>>().Add( inner );
                registry.For<IValidationAdapter<FakeTransformation>>().Add( validator );

                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( registry );
                var instance = container.GetInstance<ITransformationFactory<FakeTransformation>>();

                // outer decorator should be event inspection
                var inspection = Assert.IsType<Transformations.Internal.EventInspectionFactoryDecorator<FakeTransformation>>( instance );

                // next decorator should be throttling
                var throttling = Assert.IsType<Transformations.Internal.ThrottleableFactoryDecorator<FakeTransformation>>( inspection.inner );

                // next decorator should be cancellation
                var cancellation = Assert.IsType<Transformations.Internal.CancellationFactoryDecorator<FakeTransformation>>( throttling.inner );

                // inner decorator should be validation
                var validation = Assert.IsType<Transformations.Internal.ValidationFactoryDecorator<FakeTransformation>>( cancellation.inner );

                Assert.Same( inner, validation.inner );
                Assert.Same( validator, validation.validator );
            }
        }

        public class AddDataflowImplementations : LamarDataflowExtensionsTests
        {
            private IAssemblyScanner scanner;
            private IAssemblyScanner method() => scanner.AddDataflowImplementations();

            [Fact]
            public void requires_scanner()
            {
                scanner = null!;
                Assert.Throws<ArgumentNullException>( nameof( scanner ), method );
            }

            [Fact]
            public void registers_discovered_source_handlers()
            {
                registry.Scan( scanner =>
                {
                    this.scanner = scanner;
                    scanner.AssemblyContainingType<Dataflow>(); // sanity check to ensure decorators aren't grabbed
                    scanner.AssemblyContainingType<AddDataflowImplementations>();

                    var actual = method();
                    Assert.Same( this.scanner, actual );
                } );

                using var container = new Container( registry );
                var instance = container.GetInstance<ISourceHandler<FakeSource>>();
                Assert.IsType<LamarDataflowSourceExtensionsTests.AddSourceHandlers.FakeSourceHandler>( instance );
            }

            [Fact]
            public void registers_discovered_transformation_factories()
            {
                registry.Scan( scanner =>
                {
                    this.scanner = scanner;
                    scanner.AssemblyContainingType<Dataflow>(); // sanity check to ensure decorators aren't grabbed
                    scanner.AssemblyContainingType<AddDataflowImplementations>();

                    var actual = method();
                    Assert.Same( this.scanner, actual );
                } );

                using var container = new Container( registry );
                var instance = container.GetInstance<ITransformationFactory<FakeTransformation>>();
                Assert.IsType<LamarDataflowTransformationExtensionsTests.AddTransformationFactories.FakeTransformationFactory>( instance );
            }

            [Fact]
            public void registers_discovered_notification_receivers()
            {
                registry.Scan( scanner =>
                {
                    this.scanner = scanner;
                    scanner.AssemblyContainingType<Dataflow>(); // sanity check to ensure decorators aren't grabbed
                    scanner.AssemblyContainingType<AddDataflowImplementations>();

                    var actual = method();
                    Assert.Same( this.scanner, actual );
                } );

                using var container = new Container( registry );
                var instance = container.GetInstance<INotificationReceiver>();
                Assert.IsType<LamarDataflowNotificationExtensionsTests.AddDataflowNotifications.FakeNotificationReceiver>( instance );
            }
        }
    }
}
