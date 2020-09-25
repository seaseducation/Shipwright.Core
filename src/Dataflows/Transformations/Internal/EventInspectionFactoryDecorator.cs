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
    /// Decorates a transformation factory to add event inspection to created handlers.
    /// </summary>
    /// <typeparam name="TTransformation">Type of the transformation.</typeparam>

    public class EventInspectionFactoryDecorator<TTransformation> : TransformationFactory<TTransformation> where TTransformation : Transformation
    {
        internal readonly ITransformationFactory<TTransformation> inner;

        /// <summary>
        /// Decorates a transformation factory to add event inspection to created handlers.
        /// </summary>
        /// <param name="inner">Factory to decorate.</param>

        public EventInspectionFactoryDecorator( ITransformationFactory<TTransformation> inner )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
        }

        /// <summary>
        /// Adds a decorator to the created handler.
        /// </summary>

        protected override async Task<ITransformationHandler> Create( TTransformation transformation, CancellationToken cancellationToken )
        {
            return new EventInspectionHandlerDecorator( await inner.Create( transformation, cancellationToken ) );
        }
    }
}
