// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.AggregateTransformation;

namespace Shipwright.Dataflows.Transformations.AggregateTransformationTests
{
    public class FactoryTests
    {
        private ITransformationDispatcher dispatcher;
        private ITransformationFactory<AggregateTransformation> instance() => new Factory( dispatcher );

        private readonly Mock<ITransformationDispatcher> mockDispatcher;

        public FactoryTests()
        {
            mockDispatcher = Mockery.Of( out dispatcher );
        }

        public class Constructor : FactoryTests
        {
            [Fact]
            public void requires_dispatcher()
            {
                dispatcher = null!;
                Assert.Throws<ArgumentNullException>( nameof( dispatcher ), instance );
            }
        }

        public class Create : FactoryTests
        {
            private AggregateTransformation transformation = new AggregateTransformation { };
            private CancellationToken cancellationToken;
            private Task<ITransformationHandler> method() => instance().Create( transformation, cancellationToken );

            [Fact]
            public async Task requires_transformation()
            {
                transformation = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( transformation ), method );
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_aggregate_handler( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var handlers = new List<ITransformationHandler>();

                for ( var i = 0; i < 3; i++ )
                {
                    var child = new FakeTransformation();
                    var handler = Mockery.Of<ITransformationHandler>();
                    handlers.Add( handler );
                    transformation.Transformations.Add( child );

                    mockDispatcher.Setup( _ => _.Create( child, cancellationToken ) ).ReturnsAsync( handler );
                }

                var actual = await method();
                var typed = Assert.IsType<Handler>( actual );
                Assert.Equal( handlers, typed.handlers );
            }

            [Fact]
            public async Task disposes_created_handlers_on_exception()
            {
                var mockHandlers = new List<Mock<ITransformationHandler>>();

                for ( var i = 0; i < 3; i++ )
                {
                    var child = new FakeTransformation();
                    var mockHandler = Mockery.Of( out ITransformationHandler handler );
                    mockHandlers.Add( mockHandler );
                    transformation.Transformations.Add( child );

                    mockHandler.Setup( _ => _.DisposeAsync() ).Returns( ValueTask.CompletedTask );
                    mockDispatcher.Setup( _ => _.Create( child, cancellationToken ) ).ReturnsAsync( handler );
                }

                // force a failure
                {
                    var child = new FakeTransformation();
                    transformation.Transformations.Add( child );
                    mockDispatcher.Setup( _ => _.Create( child, cancellationToken ) ).ThrowsAsync( new OperationCanceledException() );
                }

                var ex = await Assert.ThrowsAsync<OperationCanceledException>( method );

                foreach ( var mockHandler in mockHandlers )
                {
                    mockHandler.Verify( _ => _.DisposeAsync(), Times.Once() );
                }
            }
        }
    }
}
