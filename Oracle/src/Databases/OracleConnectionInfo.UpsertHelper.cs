﻿// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Collections.Generic;
using static Shipwright.Dataflows.Transformations.DbUpsert;

namespace Shipwright.Databases
{
    public partial record OracleConnectionInfo
    {
        /// <summary>
        /// Helper for formatting query statements.
        /// </summary>
        /// <remarks>
        /// Note that the upsert component cannot be used for columns containing blob/clob data types.
        /// </remarks>

        public class UpsertHelper : Helper
        {
            /// <summary>
            /// Builds the SQL select statement to query a single existing record in the table.
            /// </summary>
            /// <param name="table">Name of the table from which to select.</param>
            /// <param name="columnsToSelect">Columns to include in the select statement.</param>
            /// <param name="keyMap">Key columns, parameter names, and current values for building conditional statements.</param>
            /// <returns>The select statement to return a single record.</returns>

            public override string BuildSelectSql( string table, IEnumerable<string> columnsToSelect, IEnumerable<ColumnValue> keyMap )
            {
                var conditions = new List<string>();

                foreach ( var (column, parameter, value) in keyMap )
                {
                    conditions.Add( $"{column} {(value == null ? "is null" : $"= :{parameter}")}" );
                }

                return $@"
                    select {string.Join( ", ", columnsToSelect )}
                    from {table}
                    where {string.Join( " and ", conditions )}";
            }

            /// <summary>
            /// Builds the SQL statement to insert a record with the given column mapping.
            /// </summary>
            /// <param name="table">Name of the table into which to insert.</param>
            /// <param name="map">Insertable columns, parameter name, and current values to include in the insert statement.</param>
            /// <returns>The insert statement to insert a single record.</returns>

            public override string BuildInsertSql( string table, IEnumerable<ColumnValue> map )
            {
                var columns = new List<string>();
                var values = new List<string>();

                foreach ( var (column, parameter, value) in map )
                {
                    columns.Add( column );
                    values.Add( $":{parameter}" );
                }

                return $@"
                    insert into {table} ( {string.Join( ", ", columns )} )
                    values ( {string.Join( ", ", values )} )";
            }

            /// <summary>
            /// Builds the SQL statement to update a record with the given column mappings.
            /// </summary>
            /// <param name="table">Table in which to update the record.</param>
            /// <param name="updateMap">Updatable columns, parameter names, and current values.</param>
            /// <param name="keyMap">Key columns, parameter names, and current values for building conditional statements.</param>
            /// <returns>The statement to update an existing database record.</returns>

            public override string BuildUpdateSql( string table, IEnumerable<ColumnValue> updateMap, IEnumerable<ColumnValue> keyMap )
            {
                var updates = new List<string>();
                var conditions = new List<string>();

                foreach ( var (column, parameter, value) in keyMap )
                {
                    conditions.Add( $"{column} {(value == null ? "is null" : $"= :{parameter}")}" );
                }

                foreach ( var (column, parameter, value) in updateMap )
                {
                    updates.Add( $"{column} = :{parameter}" );
                }

                return $@"
                    update {table}
                    set {string.Join( ", ", updates )}
                    where {string.Join( " and ", conditions )}";
            }
        }
    }
}
