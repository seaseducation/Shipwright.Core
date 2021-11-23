// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture.Xunit2;
using CsvHelper.Configuration;
using FluentValidation;
using System.Globalization;
using Xunit;
using static Shipwright.Dataflows.Sources.CsvSource;

namespace Shipwright.Dataflows.Sources.CsvSourceTests
{
    public class ValidatorTests
    {
        private readonly IValidator<CsvSource> validator = new Validator();

        public class Path : ValidatorTests
        {
            [Theory, ClassData( typeof( Cases.NullAndWhiteSpaceCases ) )]
            public void invalid_when_null_or_whitespace( string value ) => validator.InvalidWhen( _ => _.Path, value );

            [Theory, AutoData]
            public void valid_when_given( string value ) => validator.ValidWhen( _ => _.Path, value );
        }

        public class Settings : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Settings, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.Settings, new CsvConfiguration( CultureInfo.CurrentCulture ) );
        }

        public class SkipLines : ValidatorTests
        {
            [Theory]
            [InlineData( -1 )]
            [InlineData( -2 )]
            public void invalid_when_less_than_zero( int value ) => validator.InvalidWhen( _ => _.SkipLines, value );

            [Theory]
            [InlineData( 0 )]
            [InlineData( 1 )]
            [InlineData( 2 )]
            public void valid_when_greater_or_equal_to_zero( int value ) => validator.ValidWhen( _ => _.SkipLines, value );
        }
    }
}
