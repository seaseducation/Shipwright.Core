// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    /// <summary>
    /// Transformation that executes a delegate against a dataflow record.
    /// </summary>

    public partial record Code : Transformation
    {
        /// <summary>
        /// Defines a delegate for executing code against a dataflow record.
        /// </summary>
        /// <param name="record">The dataflow record being transformed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>

        public delegate Task CodeDelegate( Record record, CancellationToken cancellationToken );

        /// <summary>
        /// Code to execute against dataflow records.
        /// </summary>

        public CodeDelegate Delegate { get; init; } = ( record, ct ) => Task.CompletedTask;
    }
}
