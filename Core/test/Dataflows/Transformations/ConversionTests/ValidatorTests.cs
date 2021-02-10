// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using FluentValidation;
using System.Collections.Generic;
using Xunit;
using static Shipwright.Dataflows.Transformations.Conversion;

namespace Shipwright.Dataflows.Transformations.ConversionTests
{
    public class ValidatorTests
    {
        private IValidator<Conversion> validator => new Validator();
        private readonly Fixture fixture = new Fixture();

        public class Fields : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Fields, null );

            [Theory, Cases.NullAndWhiteSpace]
            public void invalid_when_any_element_null_or_whitespace( string value ) => validator.InvalidWhen( _ => _.Fields, new List<string>( fixture.CreateMany<string>() ) { value } );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.Fields, new List<string>( fixture.CreateMany<string>() ) );
        }

        public class Converter : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Converter, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.Converter, Converters.Email );
        }

        public class ConversionFailedEvent : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.ConversionFailedEvent, null );

            [Fact]
            public void has_child_validator() => validator.HasChildValidator( _ => _.ConversionFailedEvent, typeof( Validator.FailureEventSettingValidator ) );
        }
    }
}
