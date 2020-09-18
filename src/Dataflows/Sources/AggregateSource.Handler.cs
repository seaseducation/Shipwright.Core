// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Shipwright.Dataflows.Sources
{
    public partial record AggregateSource
    {
        /// <summary>
        /// Handler for <see cref="AggregateSource"/>.
        /// </summary>

        public class Handler : SourceHandler<AggregateSource>
        {
            private readonly ISourceDispatcher sourceDispatcher;

            /// <summary>
            /// Constructs a handler for <see cref="AggregateSource"/>.
            /// </summary>
            /// <param name="sourceDispatcher">Source dispatcher.</param>

            public Handler( ISourceDispatcher sourceDispatcher )
            {
                this.sourceDispatcher = sourceDispatcher ?? throw new ArgumentNullException( nameof( sourceDispatcher ) );
            }

            /// <summary>
            /// Reads data from the underlying data sources.
            /// </summary>
            /// <param name="source">Aggregate source from which to read.</param>
            /// <param name="comparer">Comparer for record field names.</param>
            /// <param name="cancellationToken">Cancellation token.</param>
            /// <returns>A stream of records from the data sources.</returns>

            protected override async IAsyncEnumerable<Record> Read( AggregateSource source, StringComparer comparer, [EnumeratorCancellation] CancellationToken cancellationToken )
            {
                foreach ( var child in source.Sources )
                {
                    await foreach ( var record in sourceDispatcher.Read( child, comparer, cancellationToken ) )
                    {
                        yield return record;
                    }
                }
            }
        }
    }
}
