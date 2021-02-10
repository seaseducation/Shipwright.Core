// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;

namespace Shipwright.Dataflows.Transformations
{
    public partial record AggregateTransformation
    {
        /// <summary>
        /// Validator for <see cref="AggregateTransformation"/>.
        /// </summary>

        public class Validator : AbstractValidator<AggregateTransformation>
        {
            /// <summary>
            /// Validator for <see cref="AggregateTransformation"/>.
            /// </summary>

            public Validator()
            {
                RuleFor( _ => _.Transformations ).NotNull().NotEmpty().NoNullElements();
            }
        }
    }
}
