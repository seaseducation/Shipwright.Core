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
    /// Decorates a transformation factory to add cancellation support.
    /// </summary>
    /// <typeparam name="TTransformation">Type of the transformation to decorate.</typeparam>

    public class CancellationFactoryDecorator<TTransformation> : TransformationFactory<TTransformation> where TTransformation : Transformation
    {
        internal readonly ITransformationFactory<TTransformation> inner;

        /// <summary>
        /// Decorates a transformation factory to add cancellation support.
        /// </summary>
        /// <param name="inner">Transformation factory to decorate.</param>

        public CancellationFactoryDecorator( ITransformationFactory<TTransformation> inner )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
        }

        /// <summary>
        /// Throws an exception when cancellation has been requested.
        /// </summary>
        /// <param name="transformation">Transformation whose handler to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="OperationCanceledException">Thrown when cancellation is requested.</exception>
        /// <returns>A decorated handler for the transformation.</returns>

        protected override async Task<ITransformationHandler> OnCreate( TTransformation transformation, CancellationToken cancellationToken )
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new CancellationHandlerDecorator( await inner.Create( transformation, cancellationToken ) );
        }
    }
}
