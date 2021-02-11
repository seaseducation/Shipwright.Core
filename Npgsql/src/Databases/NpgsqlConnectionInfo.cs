// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

namespace Shipwright.Databases
{
    /// <summary>
    /// Connection information for PostgreSQL databases.
    /// Uses the Npgsql library.
    /// </summary>

    public partial record NpgsqlConnectionInfo : DbConnectionInfo
    {
        /// <summary>
        /// Root connection string for the database server.
        /// </summary>

        public string ConnectionString { get; init; } = string.Empty;

        /// <summary>
        /// When non-null, overrides the database specified in the connection string.
        /// </summary>

        public string? Database { get; init; }
    }
}
