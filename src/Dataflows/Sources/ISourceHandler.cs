// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Collections.Generic;
using System.Threading;

namespace Shipwright.Dataflows.Sources
{
    /// <summary>
    /// Defines a handler for a dataflow record source.
    /// Derive from <see cref="SourceHandler{TSource}"/> to implement a handler for a data source.
    /// </summary>
    /// <typeparam name="TSource">Type of the dataflow source.</typeparam>

    public interface ISourceHandler<TSource> where TSource : Source
    {
        /// <summary>
        /// Reads records from the given dataflow source.
        /// </summary>
        /// <param name="source">Dataflow record source.</param>
        /// <param name="dataflow">Dataflow in which the source is read.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An awaitable stream of dataflow records.</returns>

        IAsyncEnumerable<Record> Read( TSource source, Dataflow dataflow, CancellationToken cancellationToken );
    }
}
