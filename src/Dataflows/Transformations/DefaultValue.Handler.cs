// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    public partial record DefaultValue
    {
        /// <summary>
        /// Handler for the default value transformation.
        /// </summary>

        public class Handler : TransformationHandler
        {
            internal readonly DefaultValue transformation;

            /// <summary>
            /// Constructs a handler for the default value transformation.
            /// </summary>
            /// <param name="transformation">Transformation settings.</param>

            public Handler( DefaultValue transformation )
            {
                this.transformation = transformation ?? throw new ArgumentNullException( nameof( transformation ) );
            }

            /// <summary>
            /// Adds default values to the given record.
            /// </summary>

            public override async Task Transform( Record record, CancellationToken cancellationToken )
            {
                if ( record == null ) throw new ArgumentNullException( nameof( record ) );

                foreach ( var (field, @default) in transformation.Defaults )
                {
                    // consider missing if the field is not in the data or the value is null
                    var missing = !record.Data.TryGetValue( field, out var value ) || value == null;

                    // also check for blank/whitespace text values if the option is enabled
                    missing |= transformation.DefaultOnBlank && value is string text && string.IsNullOrWhiteSpace( text );

                    if ( missing )
                    {
                        record.Data[field] = @default;
                    }
                }
            }
        }
    }
}
