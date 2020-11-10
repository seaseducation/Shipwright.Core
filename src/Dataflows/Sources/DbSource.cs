// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Databases;
using System.Collections.Generic;

namespace Shipwright.Dataflows.Sources
{
    /// <summary>
    /// Database query record source.
    /// </summary>

    public partial record DbSource : Source
    {
        /// <summary>
        /// Connection information for the database in which to execute the source query.
        /// </summary>

        public DbConnectionInfo ConnectionInfo { get; init; } = null!;

        /// <summary>
        /// Parameterized query to use as a data source.
        /// </summary>

        public string Sql { get; init; } = string.Empty;

        /// <summary>
        /// Anonymous object type that will be submitted as the query parameters.
        /// </summary>

        public object? Parameters { get; init; }

        /// <summary>
        /// Collection of fields that are output by the query and their query result column names.
        /// Column names are case sensitive.
        /// </summary>

        public ICollection<(string field, string column)> Output { get; init; } = new List<(string, string)>();
    }
}
