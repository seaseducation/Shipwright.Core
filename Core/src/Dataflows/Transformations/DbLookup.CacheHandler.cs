// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Identifiable;
using Shipwright.Databases;
using Shipwright.Services.Caching;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    public partial record DbLookup
    {
        /// <summary>
        /// Handler for the <see cref="DbLookup"/> transformation for when caching is enabled.
        /// </summary>

        public class CacheHandler : Handler
        {
            /// <summary>
            /// Handler for the <see cref="DbLookup"/> transformation for when caching is enabled.
            /// </summary>
            /// <param name="transformation">Transformation settings.</param>
            /// <param name="connectionFactory">Factory for creating database settings.</param>

            public CacheHandler( DbLookup transformation, IDbConnectionFactory connectionFactory ) :
                base( transformation, connectionFactory )
            { }

            /// <summary>
            /// Cache for database results, indexed by a GUID key representing the input parameters.
            /// </summary>

            private readonly AsyncCache<Guid, IEnumerable<dynamic>> cache = new AsyncCache<Guid, IEnumerable<dynamic>>();

            /// <summary>
            /// Namespace for computing parameter keys.
            /// </summary>

            private readonly Guid @namespace = NamedGuid.Compute( NamedGuidAlgorithm.SHA1, Guid.Empty, "ns://seas.technology/cache" );

            /// <summary>
            /// Allows <see cref="GetMatches(IDictionary{string, object?}, CancellationToken)"/> to be mocked.
            /// </summary>

            public virtual Task<IEnumerable<dynamic>> GetMatchesEx( IDictionary<string, object?> parameters, CancellationToken cancellationToken ) =>
                base.GetMatches( parameters, cancellationToken );

            /// <summary>
            /// Checks the cache for existing results for the parameters and calls the base
            /// implementation on cache miss.
            /// </summary>

            public override async Task<IEnumerable<dynamic>> GetMatches( IDictionary<string, object?> parameters, CancellationToken cancellationToken )
            {
                // sanity check that restores base functionality when caching is not desired
                if ( !transformation.CacheResults )
                {
                    return await GetMatchesEx( parameters, cancellationToken );
                }

                var json = JsonSerializer.Serialize( parameters );
                var key = NamedGuid.Compute( NamedGuidAlgorithm.SHA1, @namespace, json );

                return await cache.GetOrAdd( key, _ => GetMatchesEx( parameters, cancellationToken ) );
            }
        }
    }
}
