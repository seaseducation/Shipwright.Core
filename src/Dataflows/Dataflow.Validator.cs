// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;

namespace Shipwright.Dataflows
{
    public partial record Dataflow
    {
        /// <summary>
        /// Validator for dataflow commands.
        /// </summary>

        public class Validator : AbstractValidator<Dataflow>
        {
            /// <summary>
            /// Validator for dataflow commands.
            /// </summary>

            public Validator()
            {
                RuleFor( _ => _.Name ).NotNull().NotWhiteSpace();
                RuleFor( _ => _.Sources ).NotNull().NotEmpty().NoNullElements();
                RuleFor( _ => _.Transformations ).NotNull().NotEmpty().NoNullElements();
            }
        }
    }
}
