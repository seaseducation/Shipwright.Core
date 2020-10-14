// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using FluentValidation;
using System.Collections.Generic;
using Xunit;
using static Shipwright.Dataflows.Transformations.RequiredValue;

namespace Shipwright.Dataflows.Transformations.RequiredValueTests
{
    public class ValidatorTests
    {
        private readonly IValidator<RequiredValue> validator = new Validator();
        private readonly Fixture fixture = new Fixture();

        public class Fields : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Fields, null );

            [Theory, ClassData( typeof( Cases.NullAndWhiteSpaceCases ) )]
            public void invalid_when_null_or_whitespace_elements( string value ) => validator.InvalidWhen( _ => _.Fields, new List<string>( fixture.CreateMany<string>() ) { value } );

            [Fact]
            public void valid_when_empty() => validator.ValidWhen( _ => _.Fields, new List<string>() );

            [Fact]
            public void valid_when_no_null_elements() => validator.ValidWhen( _ => _.Fields, new List<string>( fixture.CreateMany<string>() ) );
        }

        public class ViolationDescription : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.ViolationDescription, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.ViolationDescription, _ => fixture.Create<string>() );
        }
    }
}
