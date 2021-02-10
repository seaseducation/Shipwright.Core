// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using AutoFixture.Xunit2;
using CsvHelper.Configuration;
using FluentValidation;
using System.Collections.Generic;
using System.Globalization;
using Xunit;
using static Shipwright.Dataflows.Transformations.CsvOutput;

namespace Shipwright.Dataflows.Transformations.CsvOutputTests
{
    public class ValidatorTests
    {
        IValidator<CsvOutput> validator = new Validator();

        public class Path : ValidatorTests
        {
            [Theory, Cases.NullAndWhiteSpace]
            public void invalid_when_null_or_whitespace( string value ) => validator.InvalidWhen( _ => _.Path, value );

            [Theory, AutoData]
            public void valid_when_given( string value ) => validator.ValidWhen( _ => _.Path, value );
        }

        public class Encoding : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Encoding, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.Encoding, System.Text.Encoding.UTF8 );
        }

        public class MaxDegreeOfParallelism : ValidatorTests
        {
            [Theory]
            [InlineData( 0 )]
            [InlineData( 2 )]
            public void invalid_when_not_one( int value ) => validator.InvalidWhen( _ => _.MaxDegreeOfParallelism, value );

            [Fact]
            public void valid_when_one() => validator.ValidWhen( _ => _.MaxDegreeOfParallelism, 1 );
        }

        public class Settings : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Settings, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.Settings, new CsvConfiguration( CultureInfo.CurrentCulture ) );
        }

        public class Output : ValidatorTests
        {
            Fixture fixture = new Fixture();

            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Output, null );

            [Theory, Cases.NullAndWhiteSpace]
            public void invalid_when_column_null_or_whitespace( string column )
            {
                var output = new List<(string, string)>( fixture.CreateMany<(string, string)>() )
                {
                    ( fixture.Create<string>(), column )
                };

                validator.InvalidWhen( _ => _.Output, output );
            }

            [Theory, ClassData( typeof( Cases.WhiteSpaceCases ) )]
            public void invalid_when_field_whitespace( string field )
            {
                var output = new List<(string, string)>( fixture.CreateMany<(string, string)>() )
                {
                    ( field, fixture.Create<string>() )
                };

                validator.InvalidWhen( _ => _.Output, output );
            }

            [Fact]
            public void valid_when_empty() => validator.ValidWhen( _ => _.Output, new List<(string, string)>() );

            [Fact]
            public void valid_when_fields_and_columns_neither_null_nor_whitespace()
            {
                var output = new List<(string, string)>( fixture.CreateMany<(string, string)>() );
                validator.ValidWhen( _ => _.Output, output );
            }

            [Fact]
            public void valid_when_field_null()
            {
                var output = new List<(string, string)>( fixture.CreateMany<(string, string)>() )
                {
                    ( null, fixture.Create<string>() )
                };

                validator.ValidWhen( _ => _.Output, output );
            }
        }
    }
}
