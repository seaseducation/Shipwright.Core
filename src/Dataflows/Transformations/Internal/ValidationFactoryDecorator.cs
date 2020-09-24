// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations.Internal
{
    /// <summary>
    /// Decorates a transformation factory to add validation support.
    /// </summary>
    /// <typeparam name="TTransformation">Type of the transformation.</typeparam>

    public class ValidationFactoryDecorator<TTransformation> : TransformationFactory<TTransformation> where TTransformation : Transformation
    {
        internal readonly ITransformationFactory<TTransformation> inner;
        internal readonly IValidationAdapter<TTransformation> validator;

        /// <summary>
        /// Decorates a transformation factory to add validation support.
        /// </summary>
        /// <param name="inner">Transformation factory to decorate.</param>
        /// <param name="validator">Validation adapter for the transformation type.</param>

        public ValidationFactoryDecorator( ITransformationFactory<TTransformation> inner, IValidationAdapter<TTransformation> validator )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
            this.validator = validator ?? throw new ArgumentNullException( nameof( validator ) );
        }

        /// <summary>
        /// Throws an exception if validation of the transformation fails.
        /// </summary>
        /// <param name="transformation">Dataflow record transformation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The handler for the transformation.</returns>
        /// <exception cref="FluentValidation.ValidationException"/>

        protected override async Task<ITransformationHandler> OnCreate( TTransformation transformation, CancellationToken cancellationToken )
        {
            await validator.ValidateAndThrow( transformation, cancellationToken );
            return await inner.Create( transformation, cancellationToken );
        }
    }
}
