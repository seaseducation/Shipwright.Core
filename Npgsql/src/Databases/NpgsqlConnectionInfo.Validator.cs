// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;

namespace Shipwright.Databases
{
    public partial record NpgsqlConnectionInfo
    {
        /// <summary>
        /// Validator for the <see cref="NpgsqlConnectionInfo"/> type.
        /// </summary>

        public class Validator : AbstractValidator<NpgsqlConnectionInfo>
        {
            /// <summary>
            /// Validator for the <see cref="NpgsqlConnectionInfo"/> type.
            /// </summary>

            public Validator()
            {
                RuleFor( _ => _.ConnectionString ).NotNull().NotWhiteSpace();
                RuleFor( _ => _.Database ).NotWhiteSpace();
            }
        }
    }
}
