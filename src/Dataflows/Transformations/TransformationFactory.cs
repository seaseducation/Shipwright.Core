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
    /// Defines a factory for creating a transformation handler.
    /// </summary>
    /// <typeparam name="TTransformation">Type of the transformation whose handler to build.</typeparam>

    public abstract class TransformationFactory<TTransformation> : ITransformationFactory<TTransformation> where TTransformation : Transformation
    {
        /// <summary>
        /// Creates a handler for the given transformation.
        /// All arguments will be non-null.
        /// </summary>
        /// <param name="transformation">Transformation whose handler to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created handler.</returns>

        protected abstract Task<ITransformationHandler> Create( TTransformation transformation, CancellationToken cancellationToken );

        /// <summary>
        /// Explicit implementation of <see cref="ITransformationFactory{TTransformation}"/>.
        /// </summary>

        async Task<ITransformationHandler> ITransformationFactory<TTransformation>.Create( TTransformation transformation, CancellationToken cancellationToken )
        {
            return transformation == null
                ? throw new ArgumentNullException( nameof( transformation ) )
                : await Create( transformation, cancellationToken );
        }
    }
}
