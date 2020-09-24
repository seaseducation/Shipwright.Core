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
    /// Defines a handler for executing a command.
    /// Use one of the available abstract handlers to implement this interface.
    /// </summary>
    /// <typeparam name="TCommand">Type of the command.</typeparam>
    /// <typeparam name="TResult">Type of the command result.</typeparam>

    public interface ICommandHandler<TCommand, TResult> where TCommand : Command<TResult>
    {
        /// <summary>
        /// Executes the given command.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the command's execution.</returns>

        Task<TResult> Execute( TCommand command, CancellationToken cancellationToken );
    }

    /// <summary>
    /// Defines a handler for executing a command type that returns no result..
    /// </summary>
    /// <typeparam name="TCommand">Type of the command.</typeparam>

    public interface ICommandHandler<TCommand> : ICommandHandler<TCommand,ValueTuple> where TCommand: Command<ValueTuple> {}
}
