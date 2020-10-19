// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

namespace Shipwright.Dataflows.Transformations
{
    public partial record DbUpsert
    {
        /// <summary>
        /// Declares a mapping between a record field and a database column.
        /// </summary>

        public record Mapping
        {
            /// <summary>
            /// Name of the record field mapped to the database column.
            /// </summary>

            public string Field { get; }

            /// <summary>
            /// Name of the database column.
            /// </summary>

            public string Column { get; }

            /// <summary>
            /// The <see cref="ColumnType"/> of the mapping.
            /// </summary>

            public ColumnType Type { get; }


            /// <summary>
            /// Maps a record field to a database column.
            /// </summary>
            /// <param name="field">Name of the record field mapped to the database column.</param>
            /// <param name="column">Name of the database column.</param>
            /// <param name="type">The <see cref="ColumnType"/> of the mapping.</param>

            public Mapping( string field, string column, ColumnType type )
            {
                Field = field;
                Column = column;
                Type = type;
            }

            /// <summary>
            /// Type deconstructor.
            /// </summary>

            public void Deconstruct( out string field, out string column, out ColumnType type )
            {
                field = Field;
                column = Column;
                type = Type;
            }
        }
    }
}
