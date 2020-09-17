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
        /// <param name="comparer">String comparer for field names.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An awaitable stream of dataflow records.</returns>

        protected abstract IAsyncEnumerable<Record> Read( TSource source, StringComparer comparer, CancellationToken cancellationToken );

        /// <summary>
        /// Explicit implementation of <see cref="ISourceHandler{TSource}"/>.
        /// </summary>

        IAsyncEnumerable<Record> ISourceHandler<TSource>.Read( TSource source, StringComparer comparer, CancellationToken cancellationToken )
        {
            return source == null ? throw new ArgumentNullException( nameof( source ) ) : Read( source, comparer, cancellationToken );
        }
    }
}
