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
    /// Decorates a command handler to add pre-execution cancellation support.
    /// </summary>
    /// <typeparam name="TCommand">Type of the command.</typeparam>
    /// <typeparam name="TResult">Type of the command result.</typeparam>

    public class CancellationDecorator<TCommand, TResult> : CommandHandler<TCommand, TResult> where TCommand : Command<TResult>
    {
        internal readonly ICommandHandler<TCommand, TResult> inner;

        /// <summary>
        /// Decorates a command handler to add pre-execution cancellation support.
        /// </summary>
        /// <param name="inner">Command handler to decorate.</param>

        public CancellationDecorator( ICommandHandler<TCommand, TResult> inner )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
        }

        /// <summary>
        /// Throws an exception if cancellation has been requested.
        /// Otherwise awaits execution of the decorated handler.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of command execution.</returns>

        protected override async Task<TResult> Execute( TCommand command, CancellationToken cancellationToken )
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await inner.Execute( command, cancellationToken );
        }
    }
}
