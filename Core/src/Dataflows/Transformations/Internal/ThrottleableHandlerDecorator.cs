// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations.Internal
{
    /// <summary>
    /// Decorates a transformation handler to limit the number of concurrent records that can be transformed.
    /// </summary>

    public class ThrottleableHandlerDecorator : TransformationHandler
    {
        internal SemaphoreSlim semaphore;
        internal ITransformationHandler inner;

        /// <summary>
        /// Decorates a transformation handler to limit the number of concurrent records that can be transformed.
        /// </summary>
        /// <param name="maxDegreeOfParallelism">Maximum number of concurrent records that may be processed.</param>
        /// <param name="inner">Transformation handler being decorated.</param>

        public ThrottleableHandlerDecorator( int maxDegreeOfParallelism, ITransformationHandler inner )
        {
            if ( maxDegreeOfParallelism <= 0 ) throw new ArgumentOutOfRangeException( nameof( maxDegreeOfParallelism ) );
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
            semaphore = new SemaphoreSlim( maxDegreeOfParallelism, maxDegreeOfParallelism );
        }

        /// <summary>
        /// Releases resources held by the instance.
        /// </summary>

        protected override async ValueTask DisposeAsyncCore()
        {
            semaphore.Dispose();
            await inner.DisposeAsync();
            await base.DisposeAsyncCore();
        }

        /// <summary>
        /// Limits the number of concurrent records that can be processed by the inner handler.
        /// </summary>

        public override async Task Transform( Record record, CancellationToken cancellationToken )
        {
            if ( record == null ) throw new ArgumentNullException( nameof( record ) );

            using ( await semaphore.LockAsync( cancellationToken ) )
            {
                await inner.Transform( record, cancellationToken );
            }
        }
    }
}
