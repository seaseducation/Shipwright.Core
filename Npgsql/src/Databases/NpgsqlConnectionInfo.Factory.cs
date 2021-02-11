// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Npgsql;
using System;
using System.Data;

namespace Shipwright.Databases
{
    public partial record NpgsqlConnectionInfo
    {
        /// <summary>
        /// Factory for generating <see cref="NpgsqlConnectionInfo"/> connections.
        /// </summary>

        public class Factory : IDbConnectionFactory
        {
            internal readonly string connectionString;

            /// <summary>
            /// Factory for generating <see cref="NpgsqlConnectionInfo"/> connections.
            /// </summary>
            /// <param name="connectionString">Completed connection string.</param>

            public Factory( string connectionString )
            {
                this.connectionString = connectionString ?? throw new ArgumentNullException( nameof( connectionString ) );
            }

            /// <summary>
            /// Creates a database connection.
            /// </summary>

            public IDbConnection Create() => new NpgsqlConnection( connectionString );
        }
    }
}
