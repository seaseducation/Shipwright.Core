// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation.Results;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FluentValidation
{
    /// <summary>
    /// Extension methods to simplify validator test expressions.
    /// </summary>

    public static class ValidatorHelpers
    {
        /// <summary>
        /// Asserts that the specified property is invalid when it has the given value.
        /// </summary>
        /// <typeparam name="TModel">Type of the partent object.</typeparam>
        /// <typeparam name="TValue">Type of the property.</typeparam>
        /// <param name="validator">Validator under test.</param>
        /// <param name="property">Property expression.</param>
        /// <param name="value">Property value that should be considered invalid.</param>

        public static IEnumerable<ValidationFailure> InvalidWhen<TModel, TValue>( this IValidator<TModel> validator, Expression<Func<TModel, TValue>> property, TValue value )
            where TModel : class, new() => validator.ShouldHaveValidationErrorFor( property, value );

        /// <summary>
        /// Asserts that the specified property is valid when it has the given value.
        /// </summary>
        /// <typeparam name="TModel">Type of the partent object.</typeparam>
        /// <typeparam name="TValue">Type of the property.</typeparam>
        /// <param name="validator">Validator under test.</param>
        /// <param name="property">Property expression.</param>
        /// <param name="value">Property value that should be considered invalid.</param>

        public static void ValidWhen<TModel, TValue>( this IValidator<TModel> validator, Expression<Func<TModel, TValue>> property, TValue value )
            where TModel : class, new() => validator.ShouldNotHaveValidationErrorFor( property, value );

        /// <summary>
        /// Asserts that the specified property has a child validator of the specified type.
        /// </summary>
        /// <typeparam name="TModel">Type of the partent object.</typeparam>
        /// <typeparam name="TValue">Type of the property.</typeparam>
        /// <param name="validator">Validator under test.</param>
        /// <param name="property">Property expression.</param>
        /// <param name="validatorType">Type of the expected child validator.</param>

        public static void HasChildValidator<TModel, TValue>( this IValidator<TModel> validator, Expression<Func<TModel, TValue>> property, Type validatorType )
            where TModel : class, new() => validator.ShouldHaveChildValidator( property, validatorType );
    }
}
