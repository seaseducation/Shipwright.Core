// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using AutoFixture.Xunit2;
using FluentValidation;
using Shipwright.Databases;
using System.Collections.Generic;
using Xunit;
using static Shipwright.Dataflows.Sources.DbSource;

namespace Shipwright.Dataflows.Sources.DbSourceTests
{
    public class ValidatorTests
    {
        private readonly IValidator<DbSource> validator = new Validator();
        private readonly Fixture fixture = new Fixture();

        public class ConnectionInfo : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.ConnectionInfo, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.ConnectionInfo, new FakeConnectionInfo() );
        }

        public class Output : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Output, null );

            [Theory, Cases.NullAndWhiteSpace]
            public void invalid_when_any_field_null_or_whitespace( string value ) =>
                validator.InvalidWhen( _ => _.Output, new List<(string, string)>( fixture.CreateMany<(string, string)>() ) { (value, fixture.Create<string>()) } );

            [Theory, Cases.NullAndWhiteSpace]
            public void invalid_when_any_column_null_or_whitespace( string value ) =>
                validator.InvalidWhen( _ => _.Output, new List<(string, string)>( fixture.CreateMany<(string, string)>() ) { (fixture.Create<string>(), value) } );

            [Fact]
            public void valid_when_empty() => validator.ValidWhen( _ => _.Output, new List<(string, string)>() );

            [Fact]
            public void valid_when_all_elements_given() => validator.ValidWhen( _ => _.Output, new List<(string, string)>( fixture.CreateMany<(string, string)>() ) );
        }

        public class Sql : ValidatorTests
        {
            [Theory, Cases.NullAndWhiteSpace]
            public void invalid_when_null_or_whitespace( string value ) => validator.InvalidWhen( _ => _.Sql, value );

            [Theory, AutoData]
            public void valid_when_given( string value ) => validator.ValidWhen( _ => _.Sql, value );
        }
    }
}
