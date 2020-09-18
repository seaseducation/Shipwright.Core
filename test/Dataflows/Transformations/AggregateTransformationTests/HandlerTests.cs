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
    public class HandlerTests
    {
        private List<ITransformationHandler> handlers = new List<ITransformationHandler>();
        private ITransformationHandler instance() => new Handler( handlers );

        public class Constructor : HandlerTests
        {
            [Fact]
            public void requires_handlers()
            {
                handlers = null!;
                Assert.Throws<ArgumentNullException>( nameof( handlers ), instance );
            }
        }

        public class DisposeAsync : HandlerTests
        {
            private ValueTask method() => instance().DisposeAsync();

            [Fact]
            public async Task disposes_children()
            {
                var mockHandlers = new List<Mock<ITransformationHandler>>();

                for ( var i = 0; i < 3; i++ )
                {
                    var mock = Mockery.Of( out ITransformationHandler handler );
                    mockHandlers.Add( mock );
                    handlers.Add( handler );
                    mock.Setup( _ => _.DisposeAsync() ).Returns( ValueTask.CompletedTask );
                }

                await method();

                foreach ( var mockHandler in mockHandlers )
                {
                    mockHandler.Verify( _ => _.DisposeAsync(), Times.Once() );
                }
            }
        }

        public class Transform : HandlerTests
        {
            private Record record = new FakeRecord();
            private CancellationToken cancellationToken;
            private Task method() => instance().Transform( record, cancellationToken );

            [Fact]
            public async Task requires_record()
            {
                record = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( record ), method );
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task executes_child_handlers_in_order( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var mockHandlers = new List<Mock<ITransformationHandler>>();
                var sequence = new MockSequence();

                for ( var i = 0; i < 3; i++ )
                {
                    var mock = Mockery.Of( out ITransformationHandler handler );
                    mockHandlers.Add( mock );
                    handlers.Add( handler );
                    mock.InSequence( sequence ).Setup( _ => _.Transform( record, cancellationToken ) ).Returns( Task.CompletedTask );
                }

                await method();

                foreach ( var mock in mockHandlers )
                {
                    mock.Verify( _ => _.Transform( record, cancellationToken ), Times.Once() );
                }
            }
        }
    }
}
