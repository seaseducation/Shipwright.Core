// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Collections.Generic;
using System.Linq;

namespace FluentValidation
{
    /// <summary>
    /// Extension methods for custom rule builders.
    /// </summary>

    public static class RuleBuilderExtensions
    {
        /// <summary>
        /// Require that a string value contain a non-whitespace value. Allows nulls.
        /// </summary>
        /// <typeparam name="T">Type of the parent object.</typeparam>
        /// <param name="builder">Rule builder.</param>

        public static IRuleBuilderOptions<T, string?> NotWhiteSpace<T>( this IRuleBuilder<T, string?> builder ) =>
            builder
                .Must( value => value == null || !string.IsNullOrWhiteSpace( value ) )
                .WithMessage( _ => Shipwright.Resources.CoreErrorMessages.NotWhiteSpaceValidationMessage );

        /// <summary>
        /// Require that a collection contain no null elements.
        /// </summary>
        /// <typeparam name="T">Type of the parent object.</typeparam>
        /// <typeparam name="V">Type of the items in the collection.</typeparam>
        /// <param name="builder">Rule builder.</param>

        public static IRuleBuilderOptions<T, IEnumerable<V>> NoNullElements<T, V>( this IRuleBuilder<T, IEnumerable<V>> builder ) =>
            builder
                .Must( collection => collection == null || collection.All( item => item != null ) )
                .WithMessage( _ => Shipwright.Resources.CoreErrorMessages.NoNullElementsValidationMessage );

        /// <summary>
        /// Require that a collection contain no whitespace elements.
        /// </summary>
        /// <typeparam name="T">Type of the parent object.</typeparam>
        /// <param name="builder">Rule builder.</param>

        public static IRuleBuilderOptions<T, IEnumerable<string>> NoWhiteSpaceElements<T>( this IRuleBuilder<T, IEnumerable<string>> builder ) =>
            builder
                .Must( collection => collection == null || collection.All( item => item == null || !string.IsNullOrWhiteSpace( item ) ) )
                .WithMessage( _ => Shipwright.Resources.CoreErrorMessages.NoWhiteSpaceElementsValidationMessage );
    }
}
