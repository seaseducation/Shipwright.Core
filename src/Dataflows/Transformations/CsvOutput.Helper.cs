// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using CsvHelper;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    partial record CsvOutput
    {
        /// <summary>
        /// Helper that wraps CsvHelper functionality.
        /// </summary>

        public class Helper : IHelper
        {
            readonly CsvOutput transformation;
            readonly bool append;
            readonly StreamWriter streamWriter;
            readonly CsvWriter csvWriter;

            /// <summary>
            /// Constructs a helper that wraps CsvHelper functionality.
            /// </summary>
            /// <param name="transformation">Transformation settings.</param>

            public Helper( CsvOutput transformation )
            {
                this.transformation = transformation ?? throw new ArgumentNullException( nameof( transformation ) );

                append = File.Exists( transformation.Path ) && transformation.Append;
                streamWriter = new StreamWriter( transformation.Path, append, transformation.Encoding );
                csvWriter = new CsvWriter( streamWriter, transformation.Settings );
            }

            /// <summary>
            /// Releases resources held by the current instance.
            /// </summary>

            public async ValueTask DisposeAsync()
            {
                await csvWriter.FlushAsync();
                await streamWriter.FlushAsync();
                await csvWriter.DisposeAsync();
                await streamWriter.DisposeAsync();
            }

            /// <summary>
            /// Writes the header record to the output file when working on a new file.
            /// </summary>

            public void WriteHeader()
            {
                if ( !append )
                {
                    foreach ( var (_, column) in transformation.Output )
                    {
                        csvWriter.WriteField( column );
                    }

                    csvWriter.NextRecord();
                }
            }

            /// <summary>
            /// Writes the given value to the next field.
            /// </summary>

            public void WriteField( object? value ) => csvWriter.WriteField( value );

            /// <summary>
            /// Advances the writer to the next record.
            /// </summary>

            public void NextRecord() => csvWriter.NextRecord();
        }
    }
}
