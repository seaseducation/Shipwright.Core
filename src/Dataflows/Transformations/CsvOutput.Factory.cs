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
        /// Factory for the <see cref="CsvOutput"/> transformation.
        /// </summary>

        public class Factory : ITransformationFactory<CsvOutput>
        {
            /// <summary>
            /// Creates a handler for the given transformation.
            /// </summary>

            public async Task<ITransformationHandler> Create( CsvOutput transformation, CancellationToken cancellationToken )
            {
                if ( transformation == null ) throw new ArgumentNullException( nameof( transformation ) );

                // ensure output directory exists
                var directory = System.IO.Path.GetDirectoryName( transformation.Path );
                System.IO.Directory.CreateDirectory( directory! );

                var helper = new Helper( transformation );
                helper.WriteHeader();

                return new Handler( transformation, helper );
            }
        }
    }
}
