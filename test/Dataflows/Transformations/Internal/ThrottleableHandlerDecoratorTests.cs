// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Dataflows.Transformations.Internal
{
    public class ThrottleableHandlerDecoratorTests
    {
        private int maxDegreeOfParallelism = 1;
        private ITransformationHandler inner;
        private ITransformationHandler instance() => new ThrottleableHandlerDecorator( maxDegreeOfParallelism, inner );

        private readonly Mock<ITransformationHandler> mockInner;

        public ThrottleableHandlerDecoratorTests()
        {
            mockInner = Mockery.Of( out inner );
        }

        public class Constructor : ThrottleableHandlerDecoratorTests
        {
            [Fact]
            public void requires_inner()
            {
                inner = null!;
                Assert.Throws<ArgumentNullException>( nameof( inner ), instance );
            }

            [Theory]
            [InlineData( 0 )]
            [InlineData( -1 )]
            public void requires_maxDop_positive( int maxDop )
            {
                maxDegreeOfParallelism = maxDop;
                Assert.Throws<ArgumentOutOfRangeException>( nameof( maxDegreeOfParallelism ), instance );
            }
        }

        public class DisposeAsync : ThrottleableHandlerDecoratorTests
        {
            private ValueTask method() => instance().DisposeAsync();

            [Fact]
            public async Task disposes_inner_handler()
            {
                mockInner.Setup( _ => _.DisposeAsync() ).Returns( ValueTask.CompletedTask );
                await method();
                mockInner.Verify( _ => _.DisposeAsync(), Times.Once() );
            }
        }

        public class Transform : ThrottleableHandlerDecoratorTests
        {
            private Record record = FakeRecord.Create();
            private CancellationToken cancellationToken;
            private Task method() => instance().Transform( record, cancellationToken );

            [Fact]
            public async Task requires_record()
            {
                record = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( record ), method );
            }

            [Fact]
            public async Task throws_when_canceled()
            {
                cancellationToken = new CancellationToken( true );
                await Assert.ThrowsAsync<TaskCanceledException>( method );
            }

            [Theory]
            [InlineData( 1 )]
            [InlineData( 2 )]
            public async Task reduces_semaphore_count_while_transforming( int maxDop )
            {
                maxDegreeOfParallelism = maxDop;
                var instance = this.instance() as ThrottleableHandlerDecorator;

                mockInner.Setup( _ => _.Transform( record, cancellationToken ) ).Returns( Task.CompletedTask )
                    .Callback( () => Assert.Equal( maxDop - 1, instance.semaphore.CurrentCount ) );

                await instance.Transform( record, cancellationToken );
                mockInner.Verify( _ => _.Transform( record, cancellationToken ), Times.Once() );
            }
        }
    }
}
