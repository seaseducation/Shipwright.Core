// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar;
using Lamar.Scanning.Conventions;
using Shipwright.Commands;
using Shipwright.Commands.Internal;
using System;

namespace Shipwright
{
    /// <summary>
    /// Extension methods for registering command types with Lamar.
    /// </summary>

    public static class LamarCommandExtensions
    {
        /// <summary>
        /// Adds the stock Shipwright command dispatcher.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddCommandDispatch( this ServiceRegistry registry )
        {
            if ( registry == null ) throw new ArgumentNullException( nameof( registry ) );

            registry.For<ICommandDispatcher>().Add<CommandDispatcher>();
            return registry;
        }

        /// <summary>
        /// Adds all implementations of <see cref="ICommandHandler{TCommand, TResult}"/> found in the scanned assemblies.
        /// </summary>
        /// <param name="scanner">Lamar assembly scanner.</param>
        /// <returns>The assembly scanner.</returns>

        public static IAssemblyScanner AddCommandHandlers( this IAssemblyScanner scanner )
        {
            if ( scanner == null ) throw new ArgumentNullException( nameof( scanner ) );

            scanner.ConnectImplementationsToTypesClosing( typeof( ICommandHandler<,> ) );
            return scanner;
        }

        /// <summary>
        /// Adds the command validation decorator to all implementations of <see cref="ICommandHandler{TCommand, TResult}"/>.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddCommandValidation( this ServiceRegistry registry )
        {
            if ( registry == null ) throw new ArgumentNullException( nameof( registry ) );

            registry.For( typeof( ICommandHandler<,> ) ).DecorateAllWith( typeof( ValidationDecorator<,> ) );
            return registry;
        }

        /// <summary>
        /// Adds the command cancellation decorator to all implementations of <see cref="ICommandHandler{TCommand, TResult}"/>.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddCommandCancellation( this ServiceRegistry registry )
        {
            if ( registry == null ) throw new ArgumentNullException( nameof( registry ) );

            registry.For( typeof( ICommandHandler<,> ) ).DecorateAllWith( typeof( CancellationDecorator<,> ) );
            return registry;
        }
    }
}
