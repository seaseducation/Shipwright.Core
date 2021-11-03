// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Databases;
using System.Collections.Generic;

namespace Shipwright.Dataflows.Transformations
{
    /// <summary>
    /// Transformation that performs a database lookup.
    /// Output mapping will be performed only when the query yields exactly one record.
    /// </summary>

    public partial record DbLookup : Transformation
    {
        /// <summary>
        /// Connection information for the database in which to perform the lookup.
        /// </summary>

        public DbConnectionInfo ConnectionInfo { get; init; } = null!;

        /// <summary>
        /// Parameterized query to use for lookup.
        /// </summary>

        public string Sql { get; init; } = string.Empty;

        /// <summary>
        /// Collection of fields participating in the query and their query parameter names.
        /// </summary>

        public ICollection<(string field, string parameter)> Input { get; init; } = new List<(string, string)>();

        /// <summary>
        /// Collection of fields that are output by the query and their query result column names.
        /// Column names are case sensitive.
        /// </summary>

        public ICollection<(string field, string column)> Output { get; init; } = new List<(string, string)>();

        /// <summary>
        /// Defines a delegate for generating an event message for lookup failures.
        /// </summary>
        /// <param name="count">Number of matching records.</param>
        /// <returns>A formatted event message.</returns>

        public delegate string FailureEventMessageDelegate( int count );

        /// <summary>
        /// Setting that defines the event logged when the query returns no results.
        /// </summary>

        public FailureEventSetting QueryZeroRecordEvent { get; init; } = new FailureEventSetting();

        /// <summary>
        /// Setting that defined the event logged when the query returns multiple results.
        /// </summary>

        public FailureEventSetting QueryMultipleRecordEvent { get; init; } = new FailureEventSetting();

        /// <summary>
        /// Whether to cache query results in memory.
        /// Defaults to false.
        /// Caching is only recommended when the same parameter values will be queried repeatedly.
        /// This can increase performance at the cost of memory pressure.
        /// </summary>

        public bool CacheResults { get; set; }

        /// <summary>
        /// Additional named values that can be added to the parameter input. 
        /// </summary>

        public ICollection<(string name, object value)> Parameters { get; init; } = new List<(string, object)>();
    }
}
