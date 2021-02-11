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
        /// Factory for the <see cref="Conversion"/> transformation.
        /// </summary>

        public class Factory : ITransformationFactory<Conversion>
        {
            /// <summary>
            /// Creates a handler for the transformation.
            /// </summary>

            public async Task<ITransformationHandler> Create( Conversion transformation, CancellationToken cancellationToken ) =>
                transformation == null ? throw new ArgumentNullException( nameof( transformation ) ) : new Handler( transformation );
        }
    }
}
