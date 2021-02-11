// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Databases
{
    public partial record OracleConnectionInfo
    {
        /// <summary>
        /// Builder for creating <see cref="OracleConnectionInfo"/> connection factories.
        /// </summary>

        public class Builder : IDbConnectionBuilder<OracleConnectionInfo>
        {
            /// <summary>
            /// Builds a connection factory for the given <see cref="OracleConnectionInfo"/>.
            /// </summary>

            public async Task<IDbConnectionFactory> Build( OracleConnectionInfo connectionInfo, CancellationToken cancellationToken )
            {
                if ( connectionInfo == null ) throw new ArgumentNullException( nameof( connectionInfo ) );

                var builder = new Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder( connectionInfo.ConnectionString );

                if ( connectionInfo.UserId != null )
                {
                    builder.UserID = connectionInfo.UserId;
                }

                if ( connectionInfo.Password != null )
                {
                    builder.Password = connectionInfo.Password;
                }

                return new Factory( builder.ToString() );
            }
        }
    }
}
