// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture.Xunit2;
using FluentValidation;
using Xunit;
using static Shipwright.Databases.SqlServerConnectionInfo;

namespace Shipwright.Databases.SqlServerConnectionInfoTests
{
    public class ValidatorTests
    {
        private readonly IValidator<SqlServerConnectionInfo> validator = new Validator();

        public class ConnectionString : ValidatorTests
        {
            [Theory, ClassData( typeof( Cases.NullAndWhiteSpaceCases ) )]
            public void invalid_when_null_or_whitespace( string value ) => validator.InvalidWhen( _ => _.ConnectionString, value );

            [Theory, AutoData]
            public void valid_when_given( string value ) => validator.ValidWhen( _ => _.ConnectionString, value );
        }

        public class Database : ValidatorTests
        {
            [Theory, ClassData( typeof( Cases.WhiteSpaceCases ) )]
            public void invalid_when_whitespace( string value ) => validator.InvalidWhen( _ => _.Database, value );

            [Fact]
            public void valid_when_null() => validator.ValidWhen( _ => _.Database, null );

            [Theory, AutoData]
            public void valid_when_given( string value ) => validator.ValidWhen( _ => _.Database, value );
        }
    }
}
