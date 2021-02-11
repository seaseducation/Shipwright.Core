// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

namespace Shipwright.Dataflows.Transformations
{
    public partial record DbUpsert
    {
        /// <summary>
        /// Type of participation a column has in an upsert operation.
        /// </summary>

        public enum ColumnType
        {
            /// <summary>
            /// Column is part of the record key used to determine uniqueness.
            /// Column value is inserted on new records, but never updated.
            /// </summary>

            Key,

            /// <summary>
            /// Column value is inserted on new records, but never updated.
            /// </summary>

            Insert,

            /// <summary>
            /// Column value is inserted on new records and updated on existing records when a
            /// change is detected.
            /// </summary>

            Upsert,

            /// <summary>
            /// Column value is updated only when a change is detected in <see cref="Upsert"/>
            /// columns. This is useful for non-deterministic fields like timestamps where you want
            /// to write a new value only when an update statement is issued.
            /// </summary>

            Trigger,
        }
    }
}
