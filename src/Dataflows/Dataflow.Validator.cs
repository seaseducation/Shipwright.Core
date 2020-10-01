// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using System.Threading.Tasks.Dataflow;

namespace Shipwright.Dataflows
{
    public partial record Dataflow
    {
        /// <summary>
        /// Defines a validator for commands that derive from <see cref="Dataflow"/>.
        /// </summary>
        /// <typeparam name="TDataflow">Derived type.</typeparam>

        public abstract class Validator<TDataflow> : AbstractValidator<TDataflow> where TDataflow : Dataflow
        {
            /// <summary>
            /// Rules for all dataflow commands.
            /// </summary>

            public Validator()
            {
                RuleFor( _ => _.Name ).NotNull().NotWhiteSpace();
                RuleFor( _ => _.Sources ).NotNull().NotEmpty().NoNullElements();
                RuleFor( _ => _.Transformations ).NotNull().NotEmpty().NoNullElements();
                RuleFor( _ => _.BufferSize ).GreaterThan( 0 ).When( _ => _.BufferSize != DataflowBlockOptions.Unbounded );
                RuleFor( _ => _.MaxDegreeOfParallelism ).GreaterThan( 0 ).When( _ => _.MaxDegreeOfParallelism != DataflowBlockOptions.Unbounded );
            }
        }

        /// <summary>
        /// Validator for the stock dataflow command.
        /// </summary>

        public class Validator : Validator<Dataflow> { }
    }
}
