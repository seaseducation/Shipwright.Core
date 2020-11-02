// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.Code;

namespace Shipwright.Dataflows.Transformations.CodeTests
{
    public class ValidatorTests
    {
        private readonly IValidator<Code> validator = new Validator();

        public class Delegate : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Delegate, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.Delegate, ( record, CancellationToken ) => Task.CompletedTask );
        }
    }
}
