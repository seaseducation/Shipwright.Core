// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using Lamar;
using Lamar.Scanning.Conventions;
using Shipwright.Validation;
using System;

namespace Shipwright
{
    /// <summary>
    /// Extension methods for registering validation types with Lamar.
    /// </summary>

    public static class LamarValidationExtensions
    {
        /// <summary>
        /// Adds the stock Shipwright adapter for FluentValidation.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddValidationAdapter( this ServiceRegistry registry )
        {
            if ( registry == null ) throw new ArgumentNullException( nameof( registry ) );

            registry.For( typeof( IValidationAdapter<> ) ).Add( typeof( ValidationAdapter<> ) );
            return registry;
        }

        /// <summary>
        /// Adds all implementations of <see cref="IValidator{T}"/> found in the scanned assemblies as singletons.
        /// </summary>
        /// <param name="scanner">Lamar assembly scanner.</param>
        /// <returns>The assembly scanner.</returns>

        public static IAssemblyScanner AddValidators( this IAssemblyScanner scanner )
        {
            if ( scanner == null ) throw new ArgumentNullException( nameof( scanner ) );

            scanner.ConnectImplementationsToTypesClosing( typeof( IValidator<> ), Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton );
            return scanner;
        }
    }
}
