// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Dapper;
using Identifiable;
using Nito.AsyncEx;
using Shipwright.Databases;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    public partial record DbUpsert
    {
        /// <summary>
        /// Handler for <see cref="DbUpsert"/> transformations.
        /// </summary>

        public class Handler : TransformationHandler
        {
            internal DbUpsert transformation;
            internal IDbConnectionFactory connectionFactory;

            /// <summary>
            /// Handler for <see cref="DbUpsert"/> transformations.
            /// </summary>

            public Handler( DbUpsert transformation, IDbConnectionFactory connectionFactory )
            {
                this.transformation = transformation ?? throw new ArgumentNullException( nameof( transformation ) );
                this.connectionFactory = connectionFactory ?? throw new ArgumentNullException( nameof( connectionFactory ) );
            }

            /// <summary>
            /// Collection of semaphores for avoiding concurrent inserts/updates to the same key.
            /// </summary>

            private readonly ConcurrentDictionary<Guid, SemaphoreSlim> semaphores = new ConcurrentDictionary<Guid, SemaphoreSlim>();

            /// <summary>
            /// Namespace for building semaphore keys.
            /// </summary>

            private Guid @namespace = NamedGuid.Compute( NamedGuidAlgorithm.SHA1, Guid.Empty, "ns://seas.technology/cache" );

            /// <summary>
            /// Releases resources held by the handler.
            /// </summary>

            protected override ValueTask DisposeAsyncCore()
            {
                foreach ( var item in semaphores )
                {
                    item.Value.Dispose();
                }

                return base.DisposeAsyncCore();
            }

            /// <summary>
            /// Builds the appropriate SQL statement for obtaining the current values of the linked
            /// database record.
            /// </summary>

            public virtual string BuildSelectSql( Record record, out IDictionary<string, object> keys )
            {
                var index = 0;
                var keyMap = new Dictionary<string, ColumnValue>();
                var columns = new List<string>();

                foreach ( var (field, column, type) in transformation.Mappings )
                {
                    if ( type == ColumnType.Key )
                    {
                        keyMap[column] = new ColumnValue( column, $"p{++index}", record.Data.TryGetValue( field, out var value ) ? value : null! );
                    }

                    if ( type == ColumnType.Upsert )
                    {
                        columns.Add( column );
                    }
                }

                keys = keyMap.Values.ToDictionary( _ => _.Parameter, _ => _.Value );
                return transformation.SqlHelper.BuildSelectSql( transformation.Table, columns, keyMap.Values );
            }

            /// <summary>
            /// Obtains a lock on the specified key value. This prevents deadlocks if the same key values
            /// appear in the dataflow more than once.
            /// </summary>
            /// <param name="keys">Record key on which to lock.</param>
            /// <param name="cancellationToken">Cancellation token.</param>
            /// <returns>A reference to a lock that releases when it is disposed.</returns>

            public virtual async Task<IDisposable> Lock( IDictionary<string, object> keys, CancellationToken cancellationToken )
            {
                var name = JsonSerializer.Serialize( keys );
                var id = NamedGuid.Compute( NamedGuidAlgorithm.SHA1, @namespace, name );
                var semaphore = semaphores.GetOrAdd( id, _ => new SemaphoreSlim( 1, 1 ) );

                return await semaphore.LockAsync( cancellationToken );
            }

            /// <summary>
            /// Executes the given query statement.
            /// </summary>
            /// <param name="sql">SQL query to execute.</param>
            /// <param name="parameters">Query parameters.</param>
            /// <param name="cancellationToken">Cancellation token.</param>
            /// <returns>The collection of query results.</returns>

            public virtual async Task<IEnumerable<dynamic>> Query( string sql, IDictionary<string, object> parameters, CancellationToken cancellationToken )
            {
                var command = new CommandDefinition( sql, parameters: parameters, cancellationToken: cancellationToken );
                using var connection = connectionFactory.Create();

                return await connection.QueryAsync( command );
            }

            /// <summary>
            /// Attempts to isolate a single result in the matches and outputs it.
            /// If multiple records are found, a failure event will be logged on the record.
            /// </summary>
            /// <param name="record">Dataflow record.</param>
            /// <param name="sql">SQL statement that produced the matches.</param>
            /// <param name="matches">Query matches.</param>
            /// <param name="keys">Key parameters that produced the error.</param>
            /// <param name="current">A single matching record or null if a record was not found.</param>
            /// <returns>
            /// True if zero or one matches were found in the results.
            /// False if multiple matches were found.
            /// </returns>

            public virtual bool TryGetCurrent( Record record, string sql, IEnumerable<dynamic> matches, IDictionary<string, object> keys, out IDictionary<string, object>? current )
            {
                var count = matches.Count();
                current = matches.FirstOrDefault();

                if ( count > 1 )
                {
                    record.Events.Add( new LogEvent
                    {
                        IsFatal = true,
                        Level = Microsoft.Extensions.Logging.LogLevel.Error,
                        Description = transformation.DuplicateKeyEventMessage( count ),
                        Value = new { sql, keys },
                    } );
                }

                return count <= 1;
            }

            /// <summary>
            /// Executes the given SQL statement.
            /// </summary>
            /// <param name="sql">SQL statement to execute.</param>
            /// <param name="parameters">Statement parameters.</param>
            /// <param name="cancellationToken">Cancellation token.</param>

            public virtual async Task Execute( string sql, IDictionary<string, object> parameters, CancellationToken cancellationToken )
            {
                var command = new CommandDefinition( sql, parameters: parameters, cancellationToken: cancellationToken );
                using var connection = connectionFactory.Create();

                await connection.ExecuteAsync( command );
            }

            /// <summary>
            /// Builds the SQL statement to perform a record insert.
            /// </summary>
            /// <param name="record">Dataflow record containing the field values.</param>
            /// <param name="parameters">Parameters for the insert statement.</param>
            /// <returns>The completed SQL statement.</returns>

            public virtual string BuildInsertSql( Record record, out IDictionary<string, object> parameters )
            {
                var index = 0;
                var map = new Dictionary<string, ColumnValue>();

                foreach ( var (field, column, type) in transformation.Mappings )
                {
                    if ( type != ColumnType.Trigger )
                    {
                        map[column] = new ColumnValue( column, $"p{++index}", record.Data.TryGetValue( field, out var value ) ? value : null! );
                    }
                }

                parameters = map.Values.ToDictionary( _ => _.Parameter, _ => _.Value );
                return transformation.SqlHelper.BuildInsertSql( transformation.Table, map.Values );
            }

            private static bool IsEqual( object source, object comparer )
            {
                // handle array comparisons (e.g. MSSQL varbinary)
                if ( source is IStructuralEquatable structural && structural.GetType().IsArray )
                {
                    return structural.Equals( comparer, StructuralComparisons.StructuralEqualityComparer );
                }

                // handle boolean/integer conversions
                // this handles edge cases where boolean values are stored in integer fields
                if ( source is bool boolSource && comparer is int intComparer )
                {
                    return (boolSource && intComparer == 1) || (!boolSource && intComparer == 0);
                }

                // handle typical comparisons
                return Equals( source, comparer );
            }

            /// <summary>
            /// Determines whether the incoming record differs from the current values in the database.
            /// </summary>
            /// <param name="record">Record containing the incoming values.</param>
            /// <param name="current">Current database values.</param>
            /// <param name="sql">The SQL statement to update the record.</param>
            /// <param name="parameters">Parameters for the update statement.</param>
            /// <returns>True if the database should be updated; otherwise false.</returns>

            public virtual bool ShouldUpdate( Record record, IDictionary<string, object> current, out string sql, out IDictionary<string, object> parameters )
            {
                var index = 0;
                var map = new Dictionary<string, ColumnValue>();
                var updates = new Dictionary<string, ColumnValue>();
                var triggers = new Dictionary<string, ColumnValue>();
                var keys = new Dictionary<string, ColumnValue>();

                ColumnValue extract( string field, string column ) =>
                    new ColumnValue( column, $"p{++index}", record.Data.TryGetValue( field, out var value ) ? value : null! );

                foreach ( var (field, column, type) in transformation.Mappings )
                {
                    if ( type == ColumnType.Key )
                    {
                        keys[column] = map[column] = extract( field, column );
                    }

                    if ( type == ColumnType.Trigger )
                    {
                        triggers[column] = map[column] = extract( field, column );
                    }

                    if ( type == ColumnType.Upsert )
                    {
                        var candidate = extract( field, column );
                        var comparer = current.TryGetValue( column, out var value ) ? value : null!;

                        if ( !IsEqual( candidate.Value, comparer ) )
                        {
                            updates[column] = map[column] = candidate;
                        }
                    }
                }

                parameters = map.Values.ToDictionary( _ => _.Parameter, _ => _.Value );
                sql = transformation.SqlHelper.BuildUpdateSql( transformation.Table, updates.Values.Union( triggers.Values ), keys.Values );
                return updates.Any();
            }

            /// <summary>
            /// Compares record values to database values and inserts or updates records as needed.
            /// </summary>

            public override async Task Transform( Record record, CancellationToken cancellationToken )
            {
                if ( record == null ) throw new ArgumentNullException( nameof( record ) );

                var select = BuildSelectSql( record, out var keys );

                using ( await Lock( keys, cancellationToken ) )
                {
                    var matches = await Query( select, keys, cancellationToken );

                    if ( TryGetCurrent( record, select, matches, keys, out var current ) )
                    {
                        if ( current == null )
                        {
                            var insert = BuildInsertSql( record, out var parameters );
                            await Execute( insert, parameters, cancellationToken );
                        }

                        else if ( ShouldUpdate( record, current, out var update, out var parameters ) )
                        {
                            await Execute( update, parameters, cancellationToken );
                        }
                    }
                }
            }
        }
    }
}
