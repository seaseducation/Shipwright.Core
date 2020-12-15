// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;

namespace Shipwright.Databases
{
    public partial record OracleConnectionInfo
    {
        /// <summary>
        /// Validator for the <see cref="OracleConnectionInfo"/> type.
        /// </summary>

        public class Validator : AbstractValidator<OracleConnectionInfo>
        {
            /// <summary>
            /// Validator for the <see cref="OracleConnectionInfo"/> type.
            /// </summary>

            public Validator()
            {
                RuleFor( _ => _.ConnectionString ).NotNull().NotWhiteSpace();
                RuleFor( _ => _.UserId ).NotWhiteSpace();
                RuleFor( _ => _.Password ).NotWhiteSpace();
            }
        }
    }
}
