// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using FluentAssertions;
using Moq;
using Shipwright.Commands;
using Shipwright.Dataflows.Notifications;
using Shipwright.Dataflows.Sources;
using Shipwright.Dataflows.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Dataflow;

namespace Shipwright.Dataflows.DataflowTests
{
    public class HandlerTests
    {
        private ISourceDispatcher sourceDispatcher;
        private ITransformationDispatcher transformationDispatcher;
        private List<INotificationReceiver> notificationReceivers = new List<INotificationReceiver>();
        private ICommandHandler<Dataflow, ValueTuple> instance() => new Handler( sourceDispatcher, transformationDispatcher, notificationReceivers );

        private readonly Mock<ISourceDispatcher> mockSourceDispatcher;
        private readonly Mock<ITransformationDispatcher> mockTransformationDispatcher;

        public HandlerTests()
        {
            mockSourceDispatcher = Mockery.Of( out sourceDispatcher );
            mockTransformationDispatcher = Mockery.Of( out transformationDispatcher );
        }

        public class Constructor : HandlerTests
        {
            [Fact]
            public void requires_sourceDispatcher()
            {
                sourceDispatcher = null!;
                Assert.Throws<ArgumentNullException>( nameof( sourceDispatcher ), instance );
            }

            [Fact]
            public void requires_transformationDispatcher()
            {
                transformationDispatcher = null!;
                Assert.Throws<ArgumentNullException>( nameof( transformationDispatcher ), instance );
            }

            [Fact]
            public void requires_notificationReceivers()
            {
                notificationReceivers = null!;
                Assert.Throws<ArgumentNullException>( nameof( notificationReceivers ), instance );
            }
        }

        public class Execute : HandlerTests
        {
            private Dataflow command = new Dataflow { BufferSize = 1, MaxDegreeOfParallelism = 1 };
            private CancellationToken cancellationToken;
            private Task<ValueTuple> method() => instance().Execute( command, cancellationToken );

            [Fact]
            public async Task requires_command()
            {
                command = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( command ), method );
            }

            /// <summary>
            /// Single-record single-notifier test to ensure everything executes in correct sequence.
            /// </summary>

            [Fact]
            public async Task executes_dataflow_in_sequence()
            {
                cancellationToken = new CancellationToken( false );

                var sequence = new MockSequence();
                var expectedRecord = FakeRecord.Create();

                // source and transformation collections
                for ( var i = 0; i < 3; i++ )
                {
                    var source = new FakeSource();
                    command.Sources.Add( source );

                    var transformation = new FakeTransformation();
                    command.Transformations.Add( transformation );
                }

                // setup transformation dispatch
                var actualTransformations = new List<Transformation>();
                var mockTransformationHandler = Mockery.Of( out ITransformationHandler transformationHandler );
                mockTransformationDispatcher.InSequence( sequence ).Setup( _ => _.Create( Capture.In( actualTransformations ), cancellationToken ) ).ReturnsAsync( transformationHandler );

                // setup dataflow start notification
                var mockReceiver = Mockery.Of( out INotificationReceiver receiver );
                notificationReceivers.Add( receiver );
                mockReceiver.InSequence( sequence ).Setup( _ => _.NotifyDataflowStarting( command, cancellationToken ) ).Returns( Task.CompletedTask );

                // setup source dispatch
                var actualSources = new List<Source>();
                mockSourceDispatcher.InSequence( sequence ).Setup( _ => _.Read( Capture.In( actualSources ), command, cancellationToken ) ).Returns( new[] { expectedRecord }.ToAsyncEnumerable() );

                // transformation handler setup
                var actualRecords = new List<Record>();
                mockTransformationHandler.InSequence( sequence ).Setup( _ => _.Transform( Capture.In( actualRecords ), cancellationToken ) ).Returns( Task.CompletedTask );

                // setup record notification
                var notifiedRecords = new List<Record>();
                mockReceiver.InSequence( sequence ).Setup( _ => _.NotifyRecordCompleted( Capture.In( notifiedRecords ), cancellationToken ) ).Returns( Task.CompletedTask );

                // setup dataflow complete notification
                mockReceiver.InSequence( sequence ).Setup( _ => _.NotifyDataflowCompleted( command, cancellationToken ) ).Returns( Task.CompletedTask );

                // setup disposal
                mockTransformationHandler.InSequence( sequence ).Setup( _ => _.DisposeAsync() ).Returns( ValueTask.CompletedTask );

                await method();

                actualSources.Should().ContainSingle().Subject.Should().BeOfType<AggregateSource>().Subject.Sources.Should().Equal( command.Sources );
                actualTransformations.Should().ContainSingle().Subject.Should().BeOfType<AggregateTransformation>().Subject.Transformations.Should().Equal( command.Transformations );
                actualRecords.Should().ContainSingle().Subject.Should().BeSameAs( expectedRecord );
                notifiedRecords.Should().BeEquivalentTo( actualRecords );

                mockTransformationHandler.Verify( _ => _.DisposeAsync(), Times.Once() );
            }

            /// <summary>
            /// Multi-record multi-notifier test to ensure all records transform and notify.
            /// </summary>

            [Fact]
            public async Task transforms_and_notifies_for_all_records()
            {
                cancellationToken = new CancellationToken( false );

                var expectedRecords = FakeRecord.Fixture().CreateMany<Record>( 3 );

                // source and transformation collections
                for ( var i = 0; i < 3; i++ )
                {
                    var source = new FakeSource();
                    command.Sources.Add( source );

                    var transformation = new FakeTransformation();
                    command.Transformations.Add( transformation );
                }

                // setup transformation dispatch
                var actualTransformations = new List<Transformation>();
                var mockTransformationHandler = Mockery.Of( out ITransformationHandler transformationHandler );
                mockTransformationDispatcher.Setup( _ => _.Create( Capture.In( actualTransformations ), cancellationToken ) ).ReturnsAsync( transformationHandler );

                // setup dataflow start notification
                var mockReceiver = Mockery.Of( out INotificationReceiver receiver );
                notificationReceivers.Add( receiver );
                mockReceiver.Setup( _ => _.NotifyDataflowStarting( command, cancellationToken ) ).Returns( Task.CompletedTask );

                // setup source dispatch
                var actualSources = new List<Source>();
                mockSourceDispatcher.Setup( _ => _.Read( Capture.In( actualSources ), command, cancellationToken ) ).Returns( expectedRecords.ToAsyncEnumerable() );

                // transformation handler setup
                var actualRecords = new List<Record>();
                mockTransformationHandler.Setup( _ => _.Transform( Capture.In( actualRecords ), cancellationToken ) ).Returns( Task.CompletedTask );

                // setup record notification
                var notifiedRecords = new List<Record>();
                mockReceiver.Setup( _ => _.NotifyRecordCompleted( Capture.In( notifiedRecords ), cancellationToken ) ).Returns( Task.CompletedTask );

                // setup dataflow complete notification
                mockReceiver.Setup( _ => _.NotifyDataflowCompleted( command, cancellationToken ) ).Returns( Task.CompletedTask );

                // setup disposal
                mockTransformationHandler.Setup( _ => _.DisposeAsync() ).Returns( ValueTask.CompletedTask );

                await method();

                actualRecords.Should().BeEquivalentTo( expectedRecords );
                notifiedRecords.Should().BeEquivalentTo( expectedRecords );
            }

            [Fact]
            public async Task cancels_when_requested()
            {
                cancellationToken = new CancellationToken( true );

                var expectedRecords = FakeRecord.Fixture().CreateMany<Record>( 3 );

                // source and transformation collections
                for ( var i = 0; i < 3; i++ )
                {
                    var source = new FakeSource();
                    command.Sources.Add( source );

                    var transformation = new FakeTransformation();
                    command.Transformations.Add( transformation );
                }

                // setup transformation dispatch
                var actualTransformations = new List<Transformation>();
                var mockTransformationHandler = Mockery.Of( out ITransformationHandler transformationHandler );
                mockTransformationDispatcher.Setup( _ => _.Create( Capture.In( actualTransformations ), cancellationToken ) ).ReturnsAsync( transformationHandler );

                // setup dataflow start notification
                var mockReceiver = Mockery.Of( out INotificationReceiver receiver );
                notificationReceivers.Add( receiver );
                mockReceiver.Setup( _ => _.NotifyDataflowStarting( command, cancellationToken ) ).Returns( Task.CompletedTask );

                // setup source dispatch
                var actualSources = new List<Source>();
                mockSourceDispatcher.Setup( _ => _.Read( Capture.In( actualSources ), command, cancellationToken ) ).Returns( expectedRecords.ToAsyncEnumerable() );

                // setup disposal
                mockTransformationHandler.Setup( _ => _.DisposeAsync() ).Returns( ValueTask.CompletedTask );

                await Assert.ThrowsAsync<TaskCanceledException>( method );

                mockTransformationHandler.Verify( _ => _.DisposeAsync(), Times.Once() );
            }
        }
    }
}
