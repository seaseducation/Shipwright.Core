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
    public class CancellationHandlerDecoratorTests
    {
        private ITransformationHandler inner;

        private ITransformationHandler instance() => new CancellationHandlerDecorator( inner );

        private readonly Mock<ITransformationHandler> mockInner;

        public CancellationHandlerDecoratorTests()
        {
            mockInner = Mockery.Of( out inner );
        }

        public class Constructor : CancellationHandlerDecoratorTests
        {
            [Fact]
            public void requires_inner()
            {
                inner = null!;
                Assert.Throws<ArgumentNullException>( nameof( inner ), instance );
            }
        }

        public class Transform : CancellationHandlerDecoratorTests
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
                await Assert.ThrowsAsync<OperationCanceledException>( method );
            }

            [Fact]
            public async Task awaits_inner_handler_when_not_canceled()
            {
                cancellationToken = new CancellationToken( false );

                mockInner.Setup( _ => _.Transform( record, cancellationToken ) ).Returns( Task.CompletedTask ).Verifiable();
                await method();

                mockInner.Verify();
            }
        }

        public class DisposeAsync : CancellationHandlerDecoratorTests
        {
            private ValueTask method() => instance().DisposeAsync();

            [Fact]
            public async ValueTask disposes_inner_handler()
            {
                mockInner.Setup( _ => _.DisposeAsync() ).Returns( ValueTask.CompletedTask );

                await method();
                mockInner.Verify( _ => _.DisposeAsync(), Times.Once() );
            }
        }
    }
}
