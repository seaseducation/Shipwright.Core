// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar;
using Lamar.Scanning.Conventions;
using Shipwright.Dataflows.Sources;
using Shipwright.Dataflows.Sources.Internal;
using System;

namespace Shipwright.Dataflows
{
    /// <summary>
    /// Extension methods for registering dataflow source types with Lamar.
    /// </summary>

    public static class LamarDataflowSourceExtensions
    {
        /// <summary>
        /// Adds the stock Shipwright dataflow record source dispatcher.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddSourceDispatch( this ServiceRegistry registry )
        {
            if ( registry == null ) throw new ArgumentNullException( nameof( registry ) );

            registry.For<ISourceDispatcher>().Add<SourceDispatcher>();
            return registry;
        }

        /// <summary>
        /// Adds all implementations of <see cref="ISourceHandler{TSource}"/> found in the scanned assemblies.
        /// </summary>
        /// <param name="scanner">Lamar assembly scanner.</param>
        /// <returns>The assembly scanner.</returns>

        public static IAssemblyScanner AddSourceHandlers( this IAssemblyScanner scanner )
        {
            if ( scanner == null ) throw new ArgumentNullException( nameof( scanner ) );

            scanner.ConnectImplementationsToTypesClosing( typeof( ISourceHandler<> ) );
            return scanner;
        }

        /// <summary>
        /// Adds the validation decorator to all implementations of <see cref="ISourceHandler{TSource}"/>.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddSourceValidation( this ServiceRegistry registry )
        {
            if ( registry == null ) throw new ArgumentNullException( nameof( registry ) );

            registry.For( typeof( ISourceHandler<> ) ).DecorateAllWith( typeof( ValidationDecorator<> ) );
            return registry;
        }

        /// <summary>
        /// Adds the cancellation decorator to all implementations of <see cref="ISourceHandler{TSource}"/>.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddSourceCancellation( this ServiceRegistry registry )
        {
            if ( registry == null ) throw new ArgumentNullException( nameof( registry ) );

            registry.For( typeof( ISourceHandler<> ) ).DecorateAllWith( typeof( CancellationDecorator<> ) );
            return registry;
        }
    }
}
