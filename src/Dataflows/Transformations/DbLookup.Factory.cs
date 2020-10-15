// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Databases;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    public partial record DbLookup
    {
        /// <summary>
        /// Factory for the <see cref="DbLookup"/> transformation.
        /// </summary>

        public class Factory : ITransformationFactory<DbLookup>
        {
            private readonly IDbConnectionDispatcher dispatcher;

            /// <summary>
            /// Factory for the <see cref="DbLookup"/> transformation.
            /// </summary>

            public Factory( IDbConnectionDispatcher dispatcher )
            {
                this.dispatcher = dispatcher ?? throw new ArgumentNullException( nameof( dispatcher ) );
            }

            /// <summary>
            /// Creates a handler for the transformation.
            /// </summary>

            public async Task<ITransformationHandler> Create( DbLookup transformation, CancellationToken cancellationToken )
            {
                if ( transformation == null ) throw new ArgumentNullException( nameof( transformation ) );

                var connectionFactory = await dispatcher.Build( transformation.ConnectionInfo, cancellationToken );

                return transformation.CacheResults
                    ? new CacheHandler( transformation, connectionFactory )
                    : new Handler( transformation, connectionFactory );
            }
        }
    }
}
