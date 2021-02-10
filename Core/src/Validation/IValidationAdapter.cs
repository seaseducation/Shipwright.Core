// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using FluentValidation.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Validation
{
    /// <summary>
    /// Defines an adapter for encapsulating validation functionality.
    /// </summary>
    /// <typeparam name="T">Type to validate.</typeparam>

    public interface IValidationAdapter<T>
    {
        /// <summary>
        /// Validates the given object instance.
        /// </summary>
        /// <param name="instance">Object instance to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The validation result.</returns>

        Task<ValidationResult> Validate( T instance, CancellationToken cancellationToken );

        /// <summary>
        /// Validates the given object instance and throws an exception if validation fails. 
        /// </summary>
        /// <param name="instance">Object instance to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>

        async Task ValidateAndThrow( T instance, CancellationToken cancellationToken )
        {
            var result = await Validate( instance, cancellationToken );

            if ( !result.IsValid )
            {
                throw new ValidationException( result.Errors );
            }
        }
    }
}
