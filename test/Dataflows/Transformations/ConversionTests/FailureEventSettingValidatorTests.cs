// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using System;
using Xunit;
using static Shipwright.Dataflows.Transformations.Conversion;

namespace Shipwright.Dataflows.Transformations.ConversionTests
{
    internal class FailureEventSettingValidatorTests
    {
        private readonly IValidator<FailureEventSetting> validator = new Validator.FailureEventSettingValidator();

        public class FailureEventMessage : FailureEventSettingValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.FailureEventMessage, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.FailureEventMessage, _ => Guid.NewGuid().ToString() );
        }
    }
}
