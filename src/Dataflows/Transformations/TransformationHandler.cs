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
    /// </summary>

    public abstract class TransformationHandler : ITransformationHandler
    {
        #region IAsyncDisposable

        /// <summary>
        /// Performs the asynchronous cleanup of managed resources or for cascading calls to DisposeAsync.
        /// Ensure that repeated calls to implementations of this method always succeed.
        /// See https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasynccore-method
        /// </summary>

        protected virtual async ValueTask DisposeAsyncCore() { }

        /// <summary>
        /// Performs asynchronous cleanup of managed resources.
        /// </summary>

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

#pragma warning disable CA1816
            // see https://github.com/dotnet/roslyn-analyzers/issues/3675
            // pragma should be removed when referenced issue is resolved
            GC.SuppressFinalize( this );
#pragma warning restore CA1816
        }

        #endregion

        /// <summary>
        /// Executes the transformation against the given dataflow record.
        /// All implementations of this method must be thread-safe.
        /// Record will always be non-null.
        /// </summary>
        /// <param name="record">Record to transform.</param>
        /// <param name="cancellationToken">Cancellation token.</param>

        protected abstract Task Transform( Record record, CancellationToken cancellationToken );

        /// <summary>
        /// Explicit implementation of <see cref="ITransformationHandler"/>.
        /// </summary>

        async Task ITransformationHandler.Transform( Record record, CancellationToken cancellationToken )
        {
            if ( record == null ) throw new ArgumentNullException( nameof( record ) );

            await Transform( record, cancellationToken );
        }
    }
}
