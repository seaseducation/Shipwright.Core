// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Shipwright.Dataflows.Sources.Internal
{
    /// <summary>
    /// Decorates a source handler to add cancellation support.
    /// </summary>


    public class CancellationDecorator<TSource> : SourceHandler<TSource> where TSource : Source
    {
        internal readonly ISourceHandler<TSource> inner;

        /// <summary>
        /// Decorates a source handler to add cancellation support.
        /// </summary>
        /// <param name="inner">Command handler to decorate.</param>

        public CancellationDecorator( ISourceHandler<TSource> inner )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
        }

        /// <summary>
        /// Throws an exception if cancellation has been requested.
        /// </summary>
        /// <param name="source">Dataflow record source.</param>
        /// <param name="dataflow">Dataflow in which the source is read.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An awaitable stream of records.</returns>

        protected override async IAsyncEnumerable<Record> Read( TSource source, Dataflow dataflow, [EnumeratorCancellation] CancellationToken cancellationToken )
        {
            cancellationToken.ThrowIfCancellationRequested();

            await foreach ( var record in inner.Read( source, dataflow, cancellationToken ) )
            {
                yield return record;
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
