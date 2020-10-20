// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using System;
using System.Collections.Generic;
using Xunit;
using static Shipwright.Dataflows.Transformations.DefaultValue;

namespace Shipwright.Dataflows.Transformations.DefaultValueTests
{
    public class ValidatorTests
    {
        private readonly IValidator<DefaultValue> validator = new Validator();

        public class Defaults : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Defaults, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.Defaults, new List<(string, Func<object>)>() );
        }
    }
}
