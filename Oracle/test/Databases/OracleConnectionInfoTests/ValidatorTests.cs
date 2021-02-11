// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture.Xunit2;
using FluentValidation;
using Xunit;
using static Shipwright.Databases.OracleConnectionInfo;

namespace Shipwright.Databases.OracleConnectionInfoTests
{
    public class ValidatorTests
    {
        private readonly IValidator<OracleConnectionInfo> validator = new Validator();

        public class ConnectionString : ValidatorTests
        {
            [Theory, ClassData( typeof( Cases.NullAndWhiteSpaceCases ) )]
            public void invalid_when_null_or_whitespace( string value ) => validator.InvalidWhen( _ => _.ConnectionString, value );

            [Theory, AutoData]
            public void valid_when_given( string value ) => validator.ValidWhen( _ => _.ConnectionString, value );
        }

        public class UserId : ValidatorTests
        {
            [Theory, ClassData( typeof( Cases.WhiteSpaceCases ) )]
            public void invalid_when_whitespace( string value ) => validator.InvalidWhen( _ => _.UserId, value );

            [Fact]
            public void valid_when_null() => validator.ValidWhen( _ => _.UserId, null );

            [Theory, AutoData]
            public void valid_when_given( string value ) => validator.ValidWhen( _ => _.UserId, value );
        }

        public class Password : ValidatorTests
        {
            [Theory, ClassData( typeof( Cases.WhiteSpaceCases ) )]
            public void invalid_when_whitespace( string value ) => validator.InvalidWhen( _ => _.Password, value );

            [Fact]
            public void valid_when_null() => validator.ValidWhen( _ => _.Password, null );

            [Theory, AutoData]
            public void valid_when_given( string value ) => validator.ValidWhen( _ => _.Password, value );
        }
    }
}
