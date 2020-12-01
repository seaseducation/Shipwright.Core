// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Dapper;
using Shipwright.Databases;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Shipwright.Dataflows.Sources
{
    public partial record DbSource
    {
        /// <summary>
        /// Handler for the <see cref="DbSource"/> data source.
        /// </summary>

        public class Handler : ISourceHandler<DbSource>
        {
            internal readonly IDbConnectionDispatcher connectionDispatcher;

            /// <summary>
            /// Handler for the <see cref="DbSource"/> data source.
            /// </summary>

            public Handler( IDbConnectionDispatcher connectionDispatcher )
            {
                this.connectionDispatcher = connectionDispatcher ?? throw new ArgumentNullException( nameof( connectionDispatcher ) );
            }

            /// <summary>
            /// Reads a stream of records from the data source.
            /// </summary>

            public async IAsyncEnumerable<Record> Read( DbSource source, Dataflow dataflow, [EnumeratorCancellation] CancellationToken cancellationToken )
            {
                if ( source == null ) throw new ArgumentNullException( nameof( source ) );
                if ( dataflow == null ) throw new ArgumentNullException( nameof( dataflow ) );

                var position = 0;
                var command = new CommandDefinition( source.Sql, parameters: source.Parameters, cancellationToken: cancellationToken );
                var connectionFactory = await connectionDispatcher.Build( source.ConnectionInfo, cancellationToken );

                using var connection = connectionFactory.Create();
                using var reader = await connection.ExecuteReaderAsync( command );
                var columns = new Dictionary<string, int>();

                Record extract( IDataReader reader )
                {
                    // set ordinal field mappings on first record
                    if ( position == 0 )
                    {
                        for ( var i = 0; i < reader.FieldCount; i++ )
                        {
                            columns[reader.GetName( i )] = i;
                        }

                        // check for any missing columns
                        var missing = source.Output.Where( _ => !columns.ContainsKey( _.column ) ).ToList();

                        // add a warning for each missing column
                        foreach ( var (field, column) in missing )
                        {
                            dataflow.Events.Add( new LogEvent
                            {
                                IsFatal = false,
                                Level = Microsoft.Extensions.Logging.LogLevel.Warning,
                                Description = Resources.CoreErrorMessages.DbSourceColumnMissing,
                                Value = new { field, column },
                            } );
                        }
                    }

                    var data = new Dictionary<string, object>( dataflow.FieldNameComparer );

                    foreach ( var (field, column) in source.Output )
                    {
                        if ( columns.TryGetValue( column, out var ordinal ) )
                        {
                            data[field] = reader.IsDBNull( ordinal ) ? null! : reader.GetValue( ordinal );
                        }
                    }

                    return new Record( dataflow, source, data, ++position );
                }

                // read asynchronously when supported
                if ( reader is DbDataReader asyncReader )
                {
                    while ( await asyncReader.ReadAsync( cancellationToken ) )
                    {
                        yield return extract( asyncReader );
                    }
                }

                else
                {
                    while ( reader.Read() )
                    {
                        yield return extract( reader );
                    }
                }
            }
        }
    }
}
