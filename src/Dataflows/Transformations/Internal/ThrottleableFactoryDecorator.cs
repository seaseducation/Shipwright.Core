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
    /// Decorates a transformation factory to add throttling support when required.
    /// </summary>
    /// <typeparam name="TTransformation">Type of the transformation being decorated..</typeparam>

    public class ThrottleableFactoryDecorator<TTransformation> : ITransformationFactory<TTransformation> where TTransformation : Transformation
    {
        internal ITransformationFactory<TTransformation> inner;

        /// <summary>
        /// Decorates a transformation factory to add throttling support when required.
        /// </summary>
        /// <param name="inner">Transformation factory being decorated.</param>

        public ThrottleableFactoryDecorator( ITransformationFactory<TTransformation> inner )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
        }

        /// <summary>
        /// Creates the transformation handler and decorates it to be throttled when required.
        /// </summary>

        public async Task<ITransformationHandler> Create( TTransformation transformation, CancellationToken cancellationToken )
        {
            if ( transformation == null ) throw new ArgumentNullException( nameof( transformation ) );

            var handler = await inner.Create( transformation, cancellationToken );

            return transformation.MaxDegreeOfParallelism < int.MaxValue
                ? new ThrottleableHandlerDecorator( transformation.MaxDegreeOfParallelism, handler )
                : handler;
        }
    }
}
