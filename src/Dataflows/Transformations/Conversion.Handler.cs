// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    public partial record Conversion
    {
        /// <summary>
        /// Handler for the <see cref="Conversion"/> transformation.
        /// </summary>

        public class Handler : TransformationHandler
        {
            private readonly Conversion transformation;

            /// <summary>
            /// Handler for the <see cref="Conversion"/> transformation.
            /// </summary>

            public Handler( Conversion transformation )
            {
                this.transformation = transformation ?? throw new ArgumentNullException( nameof( transformation ) );
            }

            /// <summary>
            /// Attempts to convert field values.
            /// </summary>

            public override async Task Transform( Record record, CancellationToken cancellationToken )
            {
                if ( record == null ) throw new ArgumentNullException( nameof( record ) );

                foreach ( var field in transformation.Fields )
                {
                    if ( record.Data.TryGetValue( field, out var value ) && value != null )
                    {
                        if ( transformation.Converter( value, out var result ) )
                        {
                            record.Data[field] = result!;
                        }

                        else
                        {
                            if ( transformation.ConversionFailedEvent.ClearField )
                            {
                                record.Data.Remove( field );
                            }

                            record.Events.Add( new LogEvent
                            {
                                IsFatal = transformation.ConversionFailedEvent.IsFatal,
                                Level = transformation.ConversionFailedEvent.Level,
                                Description = transformation.ConversionFailedEvent.FailureEventMessage( field ),
                                Value = new { value }
                            } );
                        }
                    }
                }
            }
        }
    }
}
