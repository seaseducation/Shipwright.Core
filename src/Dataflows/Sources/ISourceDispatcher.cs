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
    /// Defines a dispatcher for delegating execution of a dataflow record source.
    /// </summary>

    public interface ISourceDispatcher
    {
        /// <summary>
        /// Reads records from the given dataflow source.
        /// </summary>
        /// <param name="source">Dataflow record source.</param>
        /// <param name="comparer">String comparer for record field names.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An awaitable stream of dataflow records.</returns>

        IAsyncEnumerable<Record> Read( Source source, StringComparer comparer, CancellationToken cancellationToken );
    }
}
