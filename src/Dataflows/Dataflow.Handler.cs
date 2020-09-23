// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Commands;
using Shipwright.Dataflows.Notifications;
using Shipwright.Dataflows.Sources;
using Shipwright.Dataflows.Transformations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Shipwright.Dataflows
{
    public partial record Dataflow
    {
        /// <summary>
        /// Handler for executing dataflow commands.
        /// </summary>

        public class Handler : CommandHandler<Dataflow>
        {
            private readonly ISourceDispatcher sourceDispatcher;
            private readonly ITransformationDispatcher transformationDispatcher;
            private readonly IEnumerable<INotificationReceiver> notificationReceivers;

            /// <summary>
            /// Constructs a handler for executing dataflow commands.
            /// </summary>
            /// <param name="sourceDispatcher">Dataflow source dispatcher.</param>
            /// <param name="transformationDispatcher">Dataflow transformation dispatcher.</param>
            /// <param name="notificationReceivers">Collection of receivers for notifications.</param>

            public Handler( ISourceDispatcher sourceDispatcher, ITransformationDispatcher transformationDispatcher, IEnumerable<INotificationReceiver> notificationReceivers )
            {
                this.sourceDispatcher = sourceDispatcher ?? throw new ArgumentNullException( nameof( sourceDispatcher ) );
                this.transformationDispatcher = transformationDispatcher ?? throw new ArgumentNullException( nameof( transformationDispatcher ) );
                this.notificationReceivers = notificationReceivers ?? throw new ArgumentNullException( nameof( notificationReceivers ) );
            }

            private async Task NotifyDataflowStarting( Dataflow dataflow, CancellationToken cancellationToken )
            {
                foreach ( var receiver in notificationReceivers )
                {
                    await receiver.NotifyDataflowStarting( dataflow, cancellationToken );
                }
            }

            private async Task NotifyDataflowCompleted( Dataflow dataflow, CancellationToken cancellationToken )
            {
                foreach ( var receiver in notificationReceivers )
                {
                    await receiver.NotifyDataflowCompleted( dataflow, cancellationToken );
                }
            }

            private async Task NotifyRecordCompleted( Record record, CancellationToken cancellationToken )
            {
                foreach ( var receiver in notificationReceivers )
                {
                    await receiver.NotifyRecordCompleted( record, cancellationToken );
                }
            }

            /// <summary>
            /// Executes the dataflow.
            /// </summary>
            /// <param name="command">Dataflow to execute.</param>
            /// <param name="cancellationToken">Cancellation token.</param>

            protected override async Task Execute( Dataflow command, CancellationToken cancellationToken )
            {
                // build the transformation handler(s)
                var source = new AggregateSource { Sources = command.Sources };
                var transformation = new AggregateTransformation { Transformations = command.Transformations };
                await using var handler = await transformationDispatcher.Create( transformation, cancellationToken );

                // block options control how many records can fit in the buffer and how many records can be transformed concurrently
                var blockOptions = new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken, MaxDegreeOfParallelism = command.MaxDegreeOfParallelism, BoundedCapacity = command.BufferSize };

                // buffer receives records read from the source and queues them for transformation.
                // action performs the transformation
                var buffer = new BufferBlock<Record>( blockOptions );
                var action = new ActionBlock<Record>( async record =>
                {
                    await handler.Transform( record, cancellationToken );
                    await NotifyRecordCompleted( record, cancellationToken );
                }, blockOptions );

                // links the buffer block to the action block for automatic record transfer
                // propagate completion so we can await the action block
                using var link = buffer.LinkTo( action, new DataflowLinkOptions { PropagateCompletion = true } );

                await NotifyDataflowStarting( command, cancellationToken );

                await foreach ( var record in sourceDispatcher.Read( source, command, cancellationToken ) )
                {
                    await buffer.SendAsync( record, cancellationToken );
                }

                buffer.Complete();
                await action.Completion;

                await NotifyDataflowCompleted( command, cancellationToken );
            }
        }
    }
}
