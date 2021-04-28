// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

#pragma warning disable CS1591

namespace Shipwright.Dataflows.Transformations
{
    public partial record DbUpsert
    {
        /// <summary>
        /// Delegate for comparing values to determine whether the target should be updated.
        /// </summary>
        /// <param name="incoming">Source value from the dataflow.</param>
        /// <param name="existing">Target value from the database.</param>
        /// <returns>True when the column should be updated, otherwise false.</returns>

        public delegate bool ShouldUpdateComparer( object? incoming, object? existing );

        /// <summary>
        /// Declares a mapping between a record field and a database column.
        /// </summary>

        public record Mapping( string Field, string Column, ColumnType Type, ShouldUpdateComparer? Comparer = null );
    }
}
