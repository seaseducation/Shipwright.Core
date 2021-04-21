// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using AutoFixture.Xunit2;
using FluentValidation;
using Shipwright.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.DbUpsert;

namespace Shipwright.Dataflows.Transformations.DbUpsertTests
{
    public class ValidatorTests
    {
        private readonly IValidator<DbUpsert> validator = new Validator();

        public class ConnectionInfo : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.ConnectionInfo, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.ConnectionInfo, new FakeConnectionInfo() );
        }

        public class Table : ValidatorTests
        {
            [Theory, Cases.NullAndWhiteSpace]
            public void invalid_when_null_or_whitespace( string value ) => validator.InvalidWhen( _ => _.Table, value );

            [Theory, AutoData]
            public void valid_when_given( string value ) => validator.ValidWhen( _ => _.Table, value );
        }

        public class SqlHelper : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.SqlHelper, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.SqlHelper, new FakeSqlHelper() );
        }

        public class Mappings : ValidatorTests
        {
            private readonly Fixture fixture = new Fixture();
            private readonly List<Mapping> valid = new List<Mapping>();

            public Mappings()
            {
                foreach ( var type in Enum.GetValues<ColumnType>() )
                {
                    valid.Add( fixture.Create<Mapping>() with { Type = type } );
                }
            }

            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Mappings, null );

            [Fact]
            public void invalid_when_empty() => validator.InvalidWhen( _ => _.Mappings, new List<Mapping>() );

            [Theory, Cases.NullAndWhiteSpace]
            public void invalid_when_any_field_null_or_whitespace( string value ) =>
                validator.InvalidWhen( _ => _.Mappings, new List<Mapping>( valid ) { fixture.Create<Mapping>() with { Field = value } } );

            [Theory, Cases.NullAndWhiteSpace]
            public void invalid_when_any_column_null_or_whitespace( string value ) =>
                validator.InvalidWhen( _ => _.Mappings, new List<Mapping>( valid ) { fixture.Create<Mapping>() with { Column = value } } );

            [Fact]
            public void invalid_when_no_keys() => validator.InvalidWhen( _ => _.Mappings, valid.Where( _ => _.Type != ColumnType.Key ).ToList() );

            [Fact]
            public void valid_when_one_key() => validator.ValidWhen( _ => _.Mappings, valid );

            [Fact]
            public void valid_when_multiple_keys() => validator.ValidWhen( _ => _.Mappings, new List<Mapping>( valid ) { fixture.Create<Mapping>() with { Type = ColumnType.Key } } );
        }

        public class DuplicateKeyEventMessage : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.DuplicateKeyEventMessage, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.DuplicateKeyEventMessage, count => $"{count}" );
        }

        public class OnInserted : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.OnInserted, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.OnInserted, ( r, ct ) => Task.CompletedTask );
        }

        public class OnUnchanged : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.OnUnchanged, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.OnUnchanged, ( r, ct ) => Task.CompletedTask );
        }

        public class OnUpdated : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.OnUpdated, null );

            [Fact]
            public void valid_when_given() => validator.ValidWhen( _ => _.OnUpdated, ( r, ct ) => Task.CompletedTask );
        }
    }
}
