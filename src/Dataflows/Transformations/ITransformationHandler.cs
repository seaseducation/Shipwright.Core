// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    /// <summary>
    /// Defines a handler for executing dataflow record transformations.
    /// Derive from <see cref="TransformationHandler"/> to implement this interface.
    /// </summary>

    public interface ITransformationHandler : IAsyncDisposable
    {
        /// <summary>
        /// Executes the transformation against the given dataflow record.
        /// </summary>
        /// <param name="record">Record to transform.</param>
        /// <param name="cancellationToken">Cancellation token.</param>

        Task Transform( Record record, CancellationToken cancellationToken );
    }
}
