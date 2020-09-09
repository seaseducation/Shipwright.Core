// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Validation
{
    /// <summary>
    /// Adapter for encapsulating validation functionality.
    /// </summary>
    /// <typeparam name="T">Type to validate.</typeparam>

    public class ValidationAdapter<T> : IValidationAdapter<T>
    {
        private readonly IEnumerable<IValidator<T>> validators;

        /// <summary>
        /// Constructs and adapter for encapsulating validation functionality.
        /// </summary>
        /// <param name="validators">Collection of validators defined for the type.</param>

        public ValidationAdapter( IEnumerable<IValidator<T>> validators )
        {
            this.validators = validators ?? throw new ArgumentNullException( nameof( validators ) );
        }

        /// <summary>
        /// Validates the given object instance.
        /// </summary>
        /// <param name="instance">Object instance to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The validation result.</returns>
        /// <exception cref="ArgumentNullException"/>

        public async Task<ValidationResult> Validate( T instance, CancellationToken cancellationToken )
        {
            if ( instance == null ) throw new ArgumentNullException( nameof( instance ) );

            if ( instance is IRequiresValidation && !validators.Any() )
            {
                throw new InvalidOperationException( string.Format( Resources.CoreErrorMessages.MissingRequiredImplementation, typeof( IValidator<T> ) ) );
            }

            var errors = new List<ValidationFailure>();

            foreach ( var validator in validators )
            {
                var result = await validator.ValidateAsync( instance, cancellationToken );

                if ( !result.IsValid )
                {
                    errors.AddRange( result.Errors );
                }
            }

            return new ValidationResult( errors );
        }
    }
}
