// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    public partial record AggregateTransformation
    {
        /// <summary>
        /// Handler for <see cref="AggregateTransformation"/>.
        /// </summary>

        public class Handler : TransformationHandler
        {
            internal readonly IEnumerable<ITransformationHandler> handlers;

            /// <summary>
            /// Constructs a handler for <see cref="AggregateTransformation"/>.
            /// </summary>

            public Handler( IEnumerable<ITransformationHandler> handlers )
            {
                this.handlers = handlers ?? throw new ArgumentNullException( nameof( handlers ) );
            }

            /// <summary>
            /// Disposes of resources held by the current instance.
            /// </summary>

            protected override async ValueTask DisposeAsyncCore()
            {
                foreach ( var handler in handlers )
                {
                    await handler.DisposeAsync();
                }

                await base.DisposeAsyncCore();
            }

            /// <summary>
            /// Executes child transformations on the given record.
            /// </summary>
            /// <param name="record">Dataflow record to transform.</param>
            /// <param name="cancellationToken">Cancellation token.</param>

            public override async Task Transform( Record record, CancellationToken cancellationToken )
            {
                if ( record == null ) throw new ArgumentNullException( nameof( record ) );

                foreach ( var handler in handlers )
                {
                    await handler.Transform( record, cancellationToken );
                }
            }
        }
    }
}
