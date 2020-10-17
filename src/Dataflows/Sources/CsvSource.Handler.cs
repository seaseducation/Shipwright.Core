// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using CsvHelper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Sources
{
    public partial record CsvSource
    {
        /// <summary>
        /// Handler for reading from a CSV file.
        /// </summary>

        public class Handler : ISourceHandler<CsvSource>
        {
            /// <summary>
            /// Reads the next record from the file.
            /// </summary>
            /// <param name="csv">CSV reader.</param>
            /// <param name="headers">
            /// Dictionary of header names indexed by position.
            /// Header names will be populated when reading the first record, if applicable.
            /// </param>
            /// <returns>True if there is a record to read; otherwise false.</returns>

            private static async Task<bool> NextRecord( IReader csv, IDictionary<int, string> headers )
            {
                var first = !csv.Context.HasBeenRead && csv.Configuration.HasHeaderRecord;
                var read = await csv.ReadAsync();

                // populate header on first record
                if ( first && read && csv.ReadHeader() )
                {
                    var record = csv.Context.HeaderRecord;

                    for ( var i = 0; i < record.Length; i++ )
                    {
                        headers[i] = record[i];
                    }

                    // advance to next record
                    read = await csv.ReadAsync();
                }

                return read;
            }

            /// <summary>
            /// Reads the data source.
            /// </summary>

            private static async IAsyncEnumerable<Record> Read( CsvSource source, Dataflow dataflow )
            {
                using var reader = File.OpenText( source.Path );
                using var csv = new CsvReader( reader, source.Settings );
                var headers = new Dictionary<int, string>();

                while ( await NextRecord( csv, headers ) )
                {
                    var record = csv.Context.Record;
                    var data = new Dictionary<string, object>( dataflow.FieldNameComparer );

                    for ( var i = 0; i < record.Length; i++ )
                    {
                        var field = headers.TryGetValue( i, out var name ) && !string.IsNullOrWhiteSpace( name ) ? name : $"Field_{i}";

                        try
                        {
                            var value = record[i];
                            data.Add( field, string.IsNullOrEmpty( value ) ? null! : value );
                        }

                        catch ( ArgumentException )
                        {
                            throw new BadDataException( csv.Context, string.Format( Resources.CsvHelperMessages.DuplicateHeaderName, field ) );
                        }
                    }

                    yield return new Record( dataflow, source, data, csv.Context.Row );
                }
            }

            /// <summary>
            /// Reads the data source.
            /// </summary>

            public async IAsyncEnumerable<Record> Read( CsvSource source, Dataflow dataflow, [EnumeratorCancellation] CancellationToken cancellationToken )
            {
                if ( source == null ) throw new ArgumentNullException( nameof( source ) );
                if ( dataflow == null ) throw new ArgumentNullException( nameof( dataflow ) );

                var enumerable = Read( source, dataflow );
                await using var enumerator = enumerable.GetAsyncEnumerator( cancellationToken );

                for ( var more = true; more; )
                {
                    try
                    {
                        more = await enumerator.MoveNextAsync();
                    }

                    catch ( Exception ex )
                    {
                        var rethrow = true;
                        var @event = new LogEvent { IsFatal = true, Level = LogLevel.Error, Description = ex.Message };

                        switch ( ex )
                        {
                            case FileNotFoundException fnf:
                                @event = @event with { Value = new { source.Path } };
                                rethrow = source.ThrowOnFileNotFound;
                                break;

                            case BadDataException bad:
                                @event = @event with { Value = new { source.Path, Line = bad.ReadingContext.RawRow } };
                                rethrow = source.ThrowOnBadData;
                                break;

                            default:
                                @event = @event with { Value = new { source.Path, Exception = ex.ToString() } };
                                break;
                        }

                        dataflow.Events.Add( @event );

                        if ( rethrow )
                        {
                            throw;
                        }
                    }

                    if ( more )
                    {
                        yield return enumerator.Current;
                    }
                }
            }
        }
    }
}
