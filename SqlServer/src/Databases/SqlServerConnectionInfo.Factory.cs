// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Data;
using System.Data.SqlClient;

namespace Shipwright.Databases
{
    public partial record SqlServerConnectionInfo
    {
        /// <summary>
        /// Factory for generating <see cref="SqlServerConnectionInfo"/> connections.
        /// </summary>

        public class Factory : IDbConnectionFactory
        {
            internal readonly string connectionString;

            /// <summary>
            /// Factory for generating <see cref="SqlServerConnectionInfo"/> connections.
            /// </summary>
            /// <param name="connectionString">Completed connection string.</param>

            public Factory( string connectionString )
            {
                this.connectionString = connectionString ?? throw new ArgumentNullException( nameof( connectionString ) );
            }

            /// <summary>
            /// Creates a database connection.
            /// </summary>

            public IDbConnection Create() => new SqlConnection( connectionString );
        }
    }
}
