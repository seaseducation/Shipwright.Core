// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar;
using Lamar.Scanning.Conventions;
using Shipwright.Databases;
using Shipwright.Databases.Internal;
using System;

namespace Shipwright
{
    /// <summary>
    /// Extension methods for registering database types with Lamar.
    /// </summary>

    public static class LamarDatabaseExtensions
    {
        /// <summary>
        /// Adds the stock Shipwright connection dispatcher.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddDbConnectionDispatch( this ServiceRegistry registry )
        {
            if ( registry == null ) throw new ArgumentNullException( nameof( registry ) );

            registry.For<IDbConnectionDispatcher>().Add<DbConnectionDispatcher>();
            return registry;
        }

        /// <summary>
        /// Adds all implementations of <see cref="IDbConnectionBuilder{TConnectionInfo}"/> found in the scanned assemblies.
        /// </summary>
        /// <param name="scanner">Lamar assembly scanner.</param>
        /// <returns>The assembly scanner.</returns>

        public static IAssemblyScanner AddDbConnectionBuilders( this IAssemblyScanner scanner )
        {
            if ( scanner == null ) throw new ArgumentNullException( nameof( scanner ) );

            scanner.ConnectImplementationsToTypesClosing( typeof( IDbConnectionBuilder<> ) );
            return scanner;
        }

        /// <summary>
        /// Adds the connection validation decorator to all implementations of <see cref="IDbConnectionBuilder{TConnectionInfo}"/>.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddDbConnectionValidation( this ServiceRegistry registry )
        {
            if ( registry == null ) throw new ArgumentNullException( nameof( registry ) );

            registry.For( typeof( IDbConnectionBuilder<> ) ).DecorateAllWith( typeof( ValidationDecorator<> ) );
            return registry;
        }

        /// <summary>
        /// Adds the cancellation decorator to all implementations of <see cref="IDbConnectionBuilder{TConnectionInfo}"/>.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddDbConnectionCancellation( this ServiceRegistry registry )
        {
            if ( registry == null ) throw new ArgumentNullException( nameof( registry ) );

            registry.For( typeof( IDbConnectionBuilder<> ) ).DecorateAllWith( typeof( CancellationDecorator<> ) );
            return registry;
        }
    }
}
