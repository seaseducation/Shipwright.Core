// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Commands.Internal
{
    /// <summary>
    /// Decorates a command handler to add pre-execution validation support.
    /// </summary>
    /// <typeparam name="TCommand">Type of the command.</typeparam>
    /// <typeparam name="TResult">Type of the command result.</typeparam>

    public class ValidationDecorator<TCommand, TResult> : CommandHandler<TCommand, TResult> where TCommand : Command<TResult>
    {
        internal readonly ICommandHandler<TCommand, TResult> inner;
        internal readonly IValidationAdapter<TCommand> validator;

        /// <summary>
        /// Decorates a command handler to add pre-execution validation support.
        /// </summary>
        /// <param name="inner">Command handler to decorate.</param>
        /// <param name="validator">Validation adapter for the command type.</param>

        public ValidationDecorator( ICommandHandler<TCommand, TResult> inner, IValidationAdapter<TCommand> validator )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
            this.validator = validator ?? throw new ArgumentNullException( nameof( validator ) );
        }

        /// <summary>
        /// Throws an exception if validation of the command fails.
        /// Otherwise awaits execution of the decorated handler.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of command execution.</returns>

        protected override async Task<TResult> Execute( TCommand command, CancellationToken cancellationToken )
        {
            await validator.ValidateAndThrow( command, cancellationToken );
            return await inner.Execute( command, cancellationToken );
        }
    }
}
