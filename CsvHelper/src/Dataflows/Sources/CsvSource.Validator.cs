// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;

namespace Shipwright.Dataflows.Sources
{
    public partial record CsvSource
    {
        /// <summary>
        /// Validator for the CSV record source.
        /// </summary>

        public class Validator : AbstractValidator<CsvSource>
        {
            /// <summary>
            /// Validator for the CSV record source.
            /// </summary>

            public Validator()
            {
                RuleFor( _ => _.Path ).NotNull().NotWhiteSpace();
                RuleFor( _ => _.Settings ).NotNull();
                RuleFor( _ => _.SkipLines ).GreaterThanOrEqualTo( 0 );
            }
        }
    }
}
