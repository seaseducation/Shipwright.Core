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
    /// Defines a dispatcher for locating and executing the handlers of commands.
    /// </summary>

    public interface ICommandDispatcher
    {
        /// <summary>
        /// Locates and awaits execution of the handler for the given command.
        /// </summary>
        /// <typeparam name="TResult">Type of the command result.</typeparam>
        /// <param name="command">Command to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of command execution.</returns>

        Task<TResult> Execute<TResult>( Command<TResult> command, CancellationToken cancellationToken );

        /// <summary>
        /// Locates and awaits execution of the handler for the given command.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>

        async Task Execute( Command command, CancellationToken cancellationToken ) =>
            await Execute<ValueTuple>( command, cancellationToken );
    }
}
