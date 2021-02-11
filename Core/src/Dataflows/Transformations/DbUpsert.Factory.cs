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
    public partial record DbUpsert
    {
        /// <summary>
        /// Factory for the <see cref="DbUpsert"/> transformation.
        /// </summary>

        public class Factory : ITransformationFactory<DbUpsert>
        {
            private readonly IDbConnectionDispatcher connectionDispatcher;

            /// <summary>
            /// Factory for the <see cref="DbUpsert"/> transformation.
            /// </summary>

            public Factory( IDbConnectionDispatcher connectionDispatcher )
            {
                this.connectionDispatcher = connectionDispatcher ?? throw new ArgumentNullException( nameof( connectionDispatcher ) );
            }

            /// <summary>
            /// Creates a handler for the given transformation.
            /// </summary>

            public async Task<ITransformationHandler> Create( DbUpsert transformation, CancellationToken cancellationToken )
            {
                if ( transformation == null ) throw new ArgumentNullException( nameof( transformation ) );

                var connectionFactory = await connectionDispatcher.Build( transformation.ConnectionInfo, cancellationToken );
                return new Handler( transformation, connectionFactory );
            }
        }
    }
}
