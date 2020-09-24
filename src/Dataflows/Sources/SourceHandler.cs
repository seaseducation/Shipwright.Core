// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Shipwright.Dataflows.Sources
{
    /// <summary>
    /// Defines a handler for a dataflow record source.
    /// </summary>
    /// <typeparam name="TSource">Type of the dataflow source.</typeparam>

    public abstract class SourceHandler<TSource> : ISourceHandler<TSource> where TSource : Source
    {
        /// <summary>
        /// Reads data from the given data source.
        /// The source argument will be non-null.
        /// </summary>
        /// <param name="source">Dataflow record source definition.</param>
        /// <param name="dataflow">Dataflow in which the source is read.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An awaitable stream of dataflow records.</returns>

        protected abstract IAsyncEnumerable<Record> OnRead( TSource source, Dataflow dataflow, CancellationToken cancellationToken );

        /// <summary>
        /// Implementation of <see cref="ISourceHandler{TSource}"/>.
        /// </summary>

        public IAsyncEnumerable<Record> Read( TSource source, Dataflow dataflow, CancellationToken cancellationToken )
        {
            if ( source == null ) throw new ArgumentNullException( nameof( source ) );
            if ( dataflow == null ) throw new ArgumentNullException( nameof( dataflow ) );

            return OnRead( source, dataflow, cancellationToken );
        }
    }
}
