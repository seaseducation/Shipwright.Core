// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Dapper;
using Shipwright.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    public partial record DbLookup
    {
        /// <summary>
        /// Handler for the <see cref="DbLookup"/> transformation.
        /// </summary>

        public class Handler : TransformationHandler
        {
            private static readonly FailureEventMessageDelegate DefaultFailureMessage = count =>
                string.Format( Resources.CoreErrorMessages.DbLookupFailureMessage, count );

            internal DbLookup transformation;
            internal IDbConnectionFactory connectionFactory;

            /// <summary>
            /// Handler for the <see cref="DbLookup"/> transformation.
            /// </summary>
            /// <param name="transformation">Transformation settings.</param>
            /// <param name="connectionFactory">Factory for creating database settings.</param>

            public Handler( DbLookup transformation, IDbConnectionFactory connectionFactory )
            {
                this.transformation = transformation ?? throw new ArgumentNullException( nameof( transformation ) );
                this.connectionFactory = connectionFactory ?? throw new ArgumentNullException( nameof( connectionFactory ) );
            }

            /// <summary>
            /// Maps values specified by input field names to a dictionary of parameter values.
            /// </summary>

            public virtual IDictionary<string, object?> MapInputs( Record record )
            {
                var parameters = new Dictionary<string, object?>();

                foreach ( var (field, param) in transformation.Input )
                {
                    parameters[param] = record.Data.TryGetValue( field, out var value )
                        ? value
                        : null;
                }

                return parameters;
            }

            /// <summary>
            /// Queries the database for matching records.
            /// </summary>

            public virtual async Task<IEnumerable<dynamic>> GetMatches( IDictionary<string, object?> parameters, CancellationToken cancellationToken )
            {
                var command = new CommandDefinition( transformation.Sql, parameters: parameters, cancellationToken: cancellationToken );

                using var connection = connectionFactory.Create();
                return await connection.QueryAsync( command );
            }

            /// <summary>
            /// Locates the match to use for output mapping.
            /// </summary>

            public virtual bool TryGetResult( Record record, IEnumerable<dynamic> matches, object parameters, out IDictionary<string, object> result )
            {
                result = matches.FirstOrDefault()!;

                if ( matches.Count() != 1 )
                {
                    var setting = matches.Count() switch
                    {
                        0 => transformation.QueryZeroRecordEvent,
                        _ => transformation.QueryMultipleRecordEvent,
                    };

                    record.Events.Add( new LogEvent
                    {
                        IsFatal = setting.IsFatal,
                        Level = setting.Level,
                        Description = (setting.FailureEventMessage ?? DefaultFailureMessage).Invoke( matches.Count() ),
                        Value = parameters,
                    } );
                }

                return matches.Count() == 1;
            }

            /// <summary>
            /// Maps the result item to output fields.
            /// </summary>

            public virtual void MapResult( Record record, IDictionary<string, object> result )
            {
                foreach ( var (field, column) in transformation.Output )
                {
                    if ( result.TryGetValue( column, out var value ) )
                    {
                        record.Data[field] = value;
                    }
                }
            }

            /// <summary>
            /// Performs a database lookup and maps query results to output fields.
            /// </summary>

            public override async Task Transform( Record record, CancellationToken cancellationToken )
            {
                if ( record == null ) throw new ArgumentNullException( nameof( record ) );

                var parameters = MapInputs( record );
                var matches = await GetMatches( parameters, cancellationToken );

                if ( TryGetResult( record, matches, parameters, out var result ) )
                {
                    MapResult( record, result );
                }
            }
        }
    }
}
