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
        /// Factory for creating handlers of <see cref="AggregateTransformation"/>.
        /// </summary>

        public class Factory : ITransformationFactory<AggregateTransformation>
        {
            private readonly ITransformationDispatcher dispatcher;

            /// <summary>
            /// Constructs a factory for creating handlers of <see cref="AggregateTransformation"/>.
            /// </summary>
            /// <param name="dispatcher">Transformation dispatcher.</param>

            public Factory( ITransformationDispatcher dispatcher )
            {
                this.dispatcher = dispatcher ?? throw new ArgumentNullException( nameof( dispatcher ) );
            }

            /// <summary>
            /// Creates an aggregate transformation handler.
            /// </summary>

            public async Task<ITransformationHandler> Create( AggregateTransformation transformation, CancellationToken cancellationToken )
            {
                if ( transformation == null ) throw new ArgumentNullException( nameof( transformation ) );

                var handlers = new List<ITransformationHandler>();
                var success = false;

                try
                {
                    foreach ( var child in transformation.Transformations )
                    {
                        handlers.Add( await dispatcher.Create( child, cancellationToken ) );
                    }

                    success = true;
                    return new Handler( handlers );
                }

                finally
                {
                    if ( !success )
                    {
                        foreach ( var handler in handlers )
                        {
                            await handler.DisposeAsync();
                        }
                    }
                }
            }
        }
    }
}
