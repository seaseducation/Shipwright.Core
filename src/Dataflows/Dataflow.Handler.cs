// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Commands;
using Shipwright.Dataflows.Sources;
using Shipwright.Dataflows.Transformations;
using System;
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

            /// <summary>
            /// Constructs a handler for executing dataflow commands.
            /// </summary>
            /// <param name="sourceDispatcher">Dataflow source dispatcher.</param>
            /// <param name="transformationDispatcher">Dataflow transformation dispatcher.</param>

            public Handler( ISourceDispatcher sourceDispatcher, ITransformationDispatcher transformationDispatcher )
            {
                this.sourceDispatcher = sourceDispatcher ?? throw new ArgumentNullException( nameof( sourceDispatcher ) );
                this.transformationDispatcher = transformationDispatcher ?? throw new ArgumentNullException( nameof( transformationDispatcher ) );
            }

            /// <summary>
            /// Executes the dataflow.
            /// </summary>
            /// <param name="command">Dataflow to execute.</param>
            /// <param name="cancellationToken">Cancellation token.</param>

            protected override async Task Execute( Dataflow command, CancellationToken cancellationToken )
            {
                // build dataflow components
                var source = new AggregateSource { Sources = command.Sources };
                var transformation = new AggregateTransformation { Transformations = command.Transformations };
                await using var handler = await transformationDispatcher.Create( transformation, cancellationToken );

                // transformation pipeline
                async Task<Record> transform( Record record )
                {
                    await handler!.Transform( record, cancellationToken );
                    return record;
                }

                var blockOptions = new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken, MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded };
                var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

                var buffer = new BufferBlock<Record>( blockOptions );
                var transformer = new TransformBlock<Record, Record>( transform, blockOptions );
                var logger = new ActionBlock<Record>( _ => { }, blockOptions );
                // todo: add extensibility point for logging

                using var link1 = buffer.LinkTo( transformer, linkOptions );
                using var link2 = transformer.LinkTo( logger, linkOptions );

                await foreach ( var record in sourceDispatcher.Read( source, command, cancellationToken ) )
                {
                    await buffer.SendAsync( record, cancellationToken );
                }

                buffer.Complete();
                await logger.Completion;

                // todo: add extensibility point to notify on dataflow completion
            }
        }
    }
}
