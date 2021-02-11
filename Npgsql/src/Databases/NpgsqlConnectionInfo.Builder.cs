// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Databases
{
    public partial record NpgsqlConnectionInfo
    {
        /// <summary>
        /// Builder for creating <see cref="NpgsqlConnectionInfo"/> connection factories.
        /// </summary>

        public class Builder : IDbConnectionBuilder<NpgsqlConnectionInfo>
        {
            /// <summary>
            /// Builds a connection factory for the given <see cref="NpgsqlConnectionInfo"/>.
            /// </summary>

            public async Task<IDbConnectionFactory> Build( NpgsqlConnectionInfo connectionInfo, CancellationToken cancellationToken )
            {
                if ( connectionInfo == null ) throw new ArgumentNullException( nameof( connectionInfo ) );

                var builder = new Npgsql.NpgsqlConnectionStringBuilder( connectionInfo.ConnectionString );

                if ( connectionInfo.Database != null )
                {
                    builder.Database = connectionInfo.Database;
                }

                return new Factory( builder.ToString() );
            }
        }
    }
}
