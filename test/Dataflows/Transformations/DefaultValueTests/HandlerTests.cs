// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.DefaultValue;

namespace Shipwright.Dataflows.Transformations.DefaultValueTests
{
    public class HandlerTests
    {
        private DefaultValue transformation = new Fixture().Create<DefaultValue>();

        private ITransformationHandler instance() => new Handler( transformation );

        public class Constructor : HandlerTests
        {
            [Fact]
            public void requires_transformation()
            {
                transformation = null!;
                Assert.Throws<ArgumentNullException>( nameof( transformation ), instance );
            }
        }

        public class Transform : HandlerTests
        {
            private Record record = FakeRecord.Create();

            private Task method() => instance().Transform( record, default );

            [Fact]
            public async Task requires_record()
            {
                record = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( record ), method );
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task defaults_value_when_missing( bool defaultOnBlank )
            {
                var fixture = FakeRecord.Fixture();

                transformation = new DefaultValue
                {
                    DefaultOnBlank = defaultOnBlank,
                    Defaults = fixture.CreateMany<string>( 3 ).Select( field => (field, (object)Guid.NewGuid()) ).ToList(),
                };

                var expected = new Dictionary<string, object>( record.Data );

                foreach ( var (field, value) in transformation.Defaults )
                {
                    expected[field] = value;
                    record.Data.Remove( field );
                }

                await method();
                record.Data.Should().BeEquivalentTo( expected );
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task defaults_value_when_null( bool defaultOnBlank )
            {
                var fixture = FakeRecord.Fixture();

                transformation = new DefaultValue
                {
                    DefaultOnBlank = defaultOnBlank,
                    Defaults = fixture.CreateMany<string>( 3 ).Select( field => (field, (object)Guid.NewGuid()) ).ToList(),
                };

                var expected = new Dictionary<string, object>( record.Data );

                foreach ( var (field, value) in transformation.Defaults )
                {
                    expected[field] = value;
                    record.Data[field] = null;
                }

                await method();
                record.Data.Should().BeEquivalentTo( expected );
            }

            [Theory, ClassData( typeof( Cases.WhiteSpaceCases ) )]
            public async Task retains_whitespace_when_not_defaulting_on_blank( string whitespace )
            {
                var fixture = FakeRecord.Fixture();

                transformation = new DefaultValue
                {
                    DefaultOnBlank = false,
                    Defaults = fixture.CreateMany<string>( 3 ).Select( field => (field, (object)Guid.NewGuid()) ).ToList(),
                };

                foreach ( var (field, value) in transformation.Defaults )
                {
                    record.Data[field] = whitespace;
                }

                // we expect that nothing will change
                var expected = new Dictionary<string, object>( record.Data );

                await method();
                record.Data.Should().BeEquivalentTo( expected );
            }

            [Theory, ClassData( typeof( Cases.WhiteSpaceCases ) )]
            public async Task replaces_whitespace_when_defaulting_on_blank( string whitespace )
            {
                var fixture = FakeRecord.Fixture();

                transformation = new DefaultValue
                {
                    DefaultOnBlank = true,
                    Defaults = fixture.CreateMany<string>( 3 ).Select( field => (field, (object)Guid.NewGuid()) ).ToList(),
                };

                var expected = new Dictionary<string, object>( record.Data );

                foreach ( var (field, value) in transformation.Defaults )
                {
                    expected[field] = value;
                    record.Data[field] = whitespace;
                }

                await method();
                record.Data.Should().BeEquivalentTo( expected );
            }
        }
    }
}
