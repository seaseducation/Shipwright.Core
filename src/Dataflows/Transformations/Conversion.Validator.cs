// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;

namespace Shipwright.Dataflows.Transformations
{
    public partial record Conversion
    {
        /// <summary>
        /// Validator for the <see cref="Conversion"/> transformation.
        /// </summary>

        public class Validator : AbstractValidator<Conversion>
        {
            /// <summary>
            /// Validator for the <see cref="FailureEventSetting"/> subtype.
            /// </summary>

            public class FailureEventSettingValidator : AbstractValidator<FailureEventSetting>
            {
                /// <summary>
                /// Validator for the <see cref="FailureEventSetting"/> subtype.
                /// </summary>

                public FailureEventSettingValidator()
                {
                    RuleFor( _ => _.FailureEventMessage ).NotNull();
                }
            }

            /// <summary>
            /// Validator for the <see cref="Conversion"/> transformation.
            /// </summary>

            public Validator()
            {
                RuleFor( _ => _.Fields ).NotNull().NoNullElements().NoWhiteSpaceElements();
                RuleFor( _ => _.Converter ).NotNull();
                RuleFor( _ => _.ConversionFailedEvent ).NotNull().SetValidator( new FailureEventSettingValidator() );
            }
        }
    }
}
