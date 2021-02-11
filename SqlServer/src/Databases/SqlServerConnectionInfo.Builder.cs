// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Databases
{
    public partial record SqlServerConnectionInfo
    {
        /// <summary>
        /// Builder for creating <see cref="SqlServerConnectionInfo"/> connection factories.
        /// </summary>

        public class Builder : IDbConnectionBuilder<SqlServerConnectionInfo>
        {
            /// <summary>
            /// Builds a connection factory for the given <see cref="SqlServerConnectionInfo"/>.
            /// </summary>

            public async Task<IDbConnectionFactory> Build( SqlServerConnectionInfo connectionInfo, CancellationToken cancellationToken )
            {
                if ( connectionInfo == null ) throw new ArgumentNullException( nameof( connectionInfo ) );

                var builder = new SqlConnectionStringBuilder( connectionInfo.ConnectionString );

                if ( connectionInfo.Database != null )
                {
                    builder.InitialCatalog = connectionInfo.Database;
                }

                return new Factory( builder.ToString() );
            }
        }
    }
}
