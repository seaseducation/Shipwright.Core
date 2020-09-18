// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations.Internal
{
    /// <summary>
    /// Decorates a transformation handler to add cancellation support.
    /// </summary>

    public sealed class CancellationHandlerDecorator : TransformationHandler
    {
        internal readonly ITransformationHandler inner;

        /// <summary>
        /// Decorates a transformation handler to add cancellation support.
        /// </summary>
        /// <param name="inner">Transformation handler to decorate.</param>

        public CancellationHandlerDecorator( ITransformationHandler inner )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
        }

        /// <summary>
        /// Implementation of <see cref="IAsyncDisposable"/>.
        /// </summary>

        protected override ValueTask DisposeAsyncCore() => inner.DisposeAsync();

        /// <summary>
        /// Throws an exception if cancellation has been requested.
        /// </summary>
        /// <param name="record">Dataflow record.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="OperationCanceledException">Thrown when cancellation has been requested.</exception>

        protected override async Task Transform( Record record, CancellationToken cancellationToken )
        {
            cancellationToken.ThrowIfCancellationRequested();
            await inner.Transform( record, cancellationToken );
        }
    }
}
