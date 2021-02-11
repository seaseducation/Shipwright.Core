// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Commands.Internal
{
    /// <summary>
    /// Dispatcher for locating and executing the handlers of commands.
    /// </summary>

    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Constructs a dispatcher for locating and executing the handlers of commands.
        /// </summary>
        /// <param name="serviceProvider">Dependency injection container or service provider.</param>
        /// <exception cref="ArgumentNullException">serviceProvider is null.</exception>

        public CommandDispatcher( IServiceProvider serviceProvider )
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException( nameof( serviceProvider ) );
        }

        /// <summary>
        /// Locates and awaits execution of the handler for the given command.
        /// </summary>
        /// <typeparam name="TResult">Type of the command result.</typeparam>
        /// <param name="command">Command to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of command execution.</returns>

        public async Task<TResult> Execute<TResult>( Command<TResult> command, CancellationToken cancellationToken )
        {
            if ( command == null ) throw new ArgumentNullException( nameof( command ) );

            var commandType = command.GetType();
            var resultType = typeof( TResult );
            var handlerType = typeof( ICommandHandler<,> ).MakeGenericType( commandType, resultType );

            dynamic handler = serviceProvider.GetService( handlerType ) ??
                throw new InvalidOperationException( string.Format( Resources.CoreErrorMessages.MissingRequiredImplementation, handlerType ) );

            // use of the dynamic type offloads the complex reflection, expression tree caching,
            // and delegate compilation to the DLR. this results in reflection overhead only applying
            // to the first call; subsequent calls perform similar to statically-compiled statements.
            return await handler.Execute( (dynamic)command, cancellationToken );
        }
    }
}
