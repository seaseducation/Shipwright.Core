// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    public partial record RequiredValue
    {
        /// <summary>
        /// Handler for the <see cref="RequiredValue"/> transformation.
        /// </summary>

        public class Handler : TransformationHandler
        {
            internal readonly RequiredValue transformation;

            /// <summary>
            /// Handler for the <see cref="RequiredValue"/> transformation.
            /// </summary>

            public Handler( RequiredValue transformation )
            {
                this.transformation = transformation ?? throw new ArgumentNullException( nameof( transformation ) );
            }

            /// <summary>
            /// Ensures the record has values in required fields.
            /// </summary>

            public override async Task Transform( Record record, CancellationToken cancellationToken )
            {
                if ( record == null ) throw new ArgumentNullException( nameof( record ) );

                foreach ( var field in transformation.Fields )
                {
                    var missing = !record.Data.TryGetValue( field, out var value ) || value == null;
                    missing |= !transformation.AllowBlank && value is string text && string.IsNullOrWhiteSpace( text );

                    if ( missing )
                    {
                        record.Data.Remove( field );
                        record.Events.Add( new LogEvent
                        {
                            IsFatal = transformation.ViolationIsFatal,
                            Level = transformation.ViolationLogLevel,
                            Description = transformation.ViolationDescription( field ),
                        } );
                    }
                }
            }
        }
    }
}
