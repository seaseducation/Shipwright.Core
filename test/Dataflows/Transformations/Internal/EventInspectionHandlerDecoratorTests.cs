// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Dataflows.Transformations.Internal
{
    public class EventInspectionHandlerDecoratorTests
    {
        private ITransformationHandler inner;
        private ITransformationHandler instance() => new EventInspectionHandlerDecorator( inner );

        private readonly Mock<ITransformationHandler> mockInner;

        public EventInspectionHandlerDecoratorTests()
        {
            mockInner = Mockery.Of( out inner );
        }

        public class Constructor : EventInspectionHandlerDecoratorTests
        {
            [Fact]
            public void requires_inner()
            {
                inner = null!;
                Assert.Throws<ArgumentNullException>( nameof( inner ), instance );
            }
        }

        public class Transform : EventInspectionHandlerDecoratorTests
        {
            private Record record = FakeRecord.Create();
            private CancellationToken cancellationToken;
            private Task method() => instance().Transform( record, cancellationToken );

            [Fact]
            public async ValueTask requires_record()
            {
                record = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( record ), method );
            }

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async ValueTask skips_inner_when_fatal_event( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var fixture = FakeRecord.Fixture();
                var events = new[] { false, false, true }
                    .Select( isFatal => fixture.Create<LogEvent>() with { IsFatal = isFatal } )
                    .OrderBy( _ => Guid.NewGuid() )
                    .ToArray();

                foreach ( var @event in events )
                {
                    record.Events.Add( @event );
                }

                await method();
                mockInner.Verify( _ => _.Transform( record, cancellationToken ), Times.Never() );
            }

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async ValueTask calls_inner_when_no_fatal_events( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var fixture = FakeRecord.Fixture();
                var events = fixture.CreateMany<LogEvent>( 3 )
                    .Select( e => e with { IsFatal = false } )
                    .OrderBy( _ => Guid.NewGuid() )
                    .ToArray();

                foreach ( var @event in events )
                {
                    record.Events.Add( @event );
                }

                mockInner.Setup( _ => _.Transform( record, cancellationToken ) ).Returns( Task.CompletedTask );
                await method();
                mockInner.Verify( _ => _.Transform( record, cancellationToken ), Times.Once() );
            }
        }

        public class DisposeAsync : EventInspectionHandlerDecoratorTests
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
