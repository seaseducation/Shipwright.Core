// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar;
using Lamar.Scanning.Conventions;
using Shipwright.Dataflows.Transformations;
using Shipwright.Dataflows.Transformations.Internal;
using System;

namespace Shipwright.Dataflows
{
    /// <summary>
    /// Extension methods for registering dataflow transformation types with Lamar.
    /// </summary>

    public static class LamarDataflowTransformationExtensions
    {
        /// <summary>
        /// Adds the stock Shipwright dataflow transformation dispatcher.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddTransformationDispatch( this ServiceRegistry registry )
        {
            if ( registry == null ) throw new ArgumentNullException( nameof( registry ) );

            registry.For<ITransformationDispatcher>().Add<TransformationDispatcher>();
            return registry;
        }

        /// <summary>
        /// Adds all implementations of <see cref="ITransformationFactory{TTransformation}"/> found in the scanned assemblies.
        /// </summary>
        /// <param name="scanner">Lamar assembly scanner.</param>
        /// <returns>The assembly scanner.</returns>

        public static IAssemblyScanner AddTransformationFactories( this IAssemblyScanner scanner )
        {
            if ( scanner == null ) throw new ArgumentNullException( nameof( scanner ) );

            scanner.ConnectImplementationsToTypesClosing( typeof( ITransformationFactory<> ) );
            return scanner;
        }

        /// <summary>
        /// Adds the cancellation decorator to all implementations of <see cref="ITransformationFactory{TTransformation}"/>.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddTransformationCancellation( this ServiceRegistry registry )
        {
            if ( registry == null ) throw new ArgumentNullException( nameof( registry ) );

            registry.For( typeof( ITransformationFactory<> ) ).DecorateAllWith( typeof( CancellationFactoryDecorator<> ) );
            return registry;
        }
    }
}
