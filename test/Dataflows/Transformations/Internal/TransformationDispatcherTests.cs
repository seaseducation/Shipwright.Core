// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Moq;
using Shipwright.Resources;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Dataflows.Transformations.Internal
{
    public class TransformationDispatcherTests
    {
        private IServiceProvider serviceProvider;
        private ITransformationDispatcher instance() => new TransformationDispatcher( serviceProvider );

        private readonly Mock<IServiceProvider> mockServiceProvider;

        public TransformationDispatcherTests()
        {
            mockServiceProvider = Mockery.Of( out serviceProvider );
        }

        public class Constructor : TransformationDispatcherTests
        {
            [Fact]
            public void requires_serviceProvider()
            {
                serviceProvider = null!;
                Assert.Throws<ArgumentNullException>( nameof( serviceProvider ), instance );
            }
        }

        public class Create : TransformationDispatcherTests
        {
            private Transformation transformation = new FakeTransformation();
            private CancellationToken cancellationToken;

            private Task<ITransformationHandler> method() => instance().Create( transformation, cancellationToken );

            [Fact]
            public async Task requires_transformation()
            {
                transformation = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( transformation ), method );
            }

            [Fact]
            public async Task throws_when_factory_not_found()
            {
                var factoryType = typeof( ITransformationFactory<FakeTransformation> );
                mockServiceProvider.Setup( _ => _.GetService( factoryType ) ).Returns( null ).Verifiable();

                var actual = await Assert.ThrowsAsync<InvalidOperationException>( method );
                Assert.Equal( string.Format( CoreErrorMessages.MissingRequiredImplementation, factoryType ), actual.Message );

                mockServiceProvider.Verify();
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_created_handler_from_factory( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var mockFactory = Mockery.Of( out ITransformationFactory<FakeTransformation> factory );
                mockServiceProvider.Setup( _ => _.GetService( typeof( ITransformationFactory<FakeTransformation> ) ) ).Returns( factory );

                var mockHandler = Mockery.Of( out ITransformationHandler handler );
                mockFactory.Setup( _ => _.Create( (FakeTransformation)transformation, cancellationToken ) ).ReturnsAsync( handler );
                mockHandler.Setup( _ => _.DisposeAsync() ).Returns( ValueTask.CompletedTask );

                await using var actual = await method();
                Assert.Same( handler, actual );
            }
        }
    }
}
