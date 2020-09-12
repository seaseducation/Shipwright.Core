// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Commands
{
    /// <summary>
    /// Defines a handler for a command type that returns a result.
    /// </summary>
    /// <typeparam name="TCommand">Type of the command.</typeparam>
    /// <typeparam name="TResult">Type of the command result.</typeparam>

    public abstract class CommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult> where TCommand : Command<TResult>
    {
        /// <summary>
        /// Executes the logic for the given command.
        /// The command argument will be non-null at this point.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the command's execution.</returns>

        protected abstract Task<TResult> Execute( TCommand command, CancellationToken cancellationToken );

        /// <summary>
        /// Explicit interface definition.
        /// This will null-check the command argument and await the specific implementation.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the command's execution.</returns>

        async Task<TResult> ICommandHandler<TCommand, TResult>.Execute( TCommand command, CancellationToken cancellationToken )
        {
            return command == null ? throw new ArgumentNullException( nameof( command ) ) : await Execute( command, cancellationToken );
        }
    }

    /// <summary>
    /// Defines a handler for a command type that does not return a result.
    /// </summary>
    /// <typeparam name="TCommand">Type of the command.</typeparam>

    public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand, ValueTuple> where TCommand : Command
    {
        /// <summary>
        /// Executes the logic for the given command.
        /// The command argument will be non-null at this point.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>

        protected abstract Task Execute( TCommand command, CancellationToken cancellationToken );

        /// <summary>
        /// Explicit interface definition.
        /// This will null-check the command argument and await the specific implementation.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A unit type that represents no result.</returns>

        async Task<ValueTuple> ICommandHandler<TCommand, ValueTuple>.Execute( TCommand command, CancellationToken cancellationToken )
        {
            if ( command == null ) throw new ArgumentNullException( nameof( command ) );

            await Execute( command, cancellationToken );
            return default;
        }
    }
}
