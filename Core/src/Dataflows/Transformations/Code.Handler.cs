// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    public partial record Code
    {
        /// <summary>
        /// Handler for the <see cref="Code"/> transformation.
        /// </summary>

        public class Handler : TransformationHandler
        {
            internal readonly Code transformation;

            /// <summary>
            /// Handler for the <see cref="Code"/> transformation.
            /// </summary>

            public Handler( Code transformation )
            {
                this.transformation = transformation ?? throw new ArgumentNullException( nameof( transformation ) );
            }

            /// <summary>
            /// Executes the transformation delegate against the given dataflow record.
            /// </summary>

            public override async Task Transform( Record record, CancellationToken cancellationToken )
            {
                if ( record == null ) throw new ArgumentNullException( nameof( record ) );

                await transformation.Delegate( record, cancellationToken );
            }
        }
    }
}
