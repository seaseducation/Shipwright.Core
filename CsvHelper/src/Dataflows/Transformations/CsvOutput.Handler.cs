// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    partial record CsvOutput
    {
        /// <summary>
        /// Handler for the <see cref="CsvOutput"/> transformation.
        /// </summary>

        public class Handler : TransformationHandler
        {
            internal readonly CsvOutput transformation;
            internal readonly IHelper helper;

            /// <summary>
            /// Handler for the <see cref="CsvOutput"/> transformation.
            /// </summary>

            public Handler( CsvOutput transformation, IHelper helper )
            {
                this.transformation = transformation ?? throw new ArgumentNullException( nameof( transformation ) );
                this.helper = helper ?? throw new ArgumentNullException( nameof( helper ) );
            }

            /// <summary>
            /// Releases resources held by the handler.
            /// </summary>

            protected override async ValueTask DisposeAsyncCore()
            {
                await helper.DisposeAsync();
                await base.DisposeAsyncCore();
            }

            /// <summary>
            /// Writes the given record to the CSV file.
            /// </summary>

            public override async Task Transform( Record record, CancellationToken cancellationToken )
            {
                if ( record == null ) throw new ArgumentNullException( nameof( record ) );

                // ensure exclusive access to the csv file
                lock ( helper )
                {
                    foreach ( var (field, column) in transformation.Output )
                    {
                        // unmapped columns receive a null value
                        if ( field == null )
                        {
                            helper.WriteField( null );
                        }

                        else
                        {
                            helper.WriteField( record.Data.TryGetValue( field, out var value ) ? value : null );
                        }
                    }

                    helper.NextRecord();
                }
            }
        }
    }
}
