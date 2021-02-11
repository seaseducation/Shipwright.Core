// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.RequiredValue;

namespace Shipwright.Dataflows.Transformations.RequiredValueTests
{
    public class HandlerTests
    {
        private RequiredValue transformation = new Fixture().Create<RequiredValue>();
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

        public class ViolationDescription
        {
            [Theory, AutoData]
            public void defaults_to_resource_message( string field )
            {
                var actual = new RequiredValue().ViolationDescription( field );
                actual.Should().Be( string.Format( Resources.CoreErrorMessages.MissingRequiredFieldValue, field ) );
            }
        }

        public class Transform : HandlerTests
        {
            private Record record = FakeRecord.Create();
            private Task method() => instance().Transform( record, default );

            private readonly Fixture fixture = new Fixture();

            [Fact]
            public async Task requires_record()
            {
                record = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( record ), method );
            }

            [Fact]
            public async Task adds_event_when_field_missing()
            {
                var description = Guid.NewGuid().ToString() + " {0}";
                var expectedEvents = new List<LogEvent>();

                transformation = transformation with
                {
                    Fields = new List<string>( fixture.CreateMany<string>( 3 ) ),
                    ViolationDescription = field => string.Format( description, field ),
                };

                foreach ( var field in transformation.Fields )
                {
                    record.Data.Remove( field );
                    expectedEvents.Add( new LogEvent
                    {
                        IsFatal = transformation.ViolationIsFatal,
                        Level = transformation.ViolationLogLevel,
                        Description = transformation.ViolationDescription( field ),
                    } );
                }

                var expectedData = new Dictionary<string, object>( record.Data );
                await method();

                record.Data.Should().BeEquivalentTo( expectedData );
                record.Events.Should().BeEquivalentTo( expectedEvents );
            }

            [Fact]
            public async Task adds_event_when_field_value_is_null()
            {
                var description = Guid.NewGuid().ToString() + " {0}";

                transformation = transformation with
                {
                    Fields = new List<string>( fixture.CreateMany<string>( 3 ) ),
                    ViolationDescription = field => string.Format( description, field ),
                };

                var expectedEvents = new List<LogEvent>();
                var expectedData = new Dictionary<string, object>( record.Data );

                foreach ( var field in transformation.Fields )
                {
                    // we expect the transformation to remove the field from the data set
                    expectedData.Remove( field );
                    record.Data[field] = null;
                    expectedEvents.Add( new LogEvent
                    {
                        IsFatal = transformation.ViolationIsFatal,
                        Level = transformation.ViolationLogLevel,
                        Description = transformation.ViolationDescription( field ),
                    } );
                }

                await method();

                record.Data.Should().BeEquivalentTo( expectedData );
                record.Events.Should().BeEquivalentTo( expectedEvents );
            }

            [Theory, ClassData( typeof( Cases.WhiteSpaceCases ) )]
            public async Task adds_event_when_field_value_is_blank_and_not_allowed( string value )
            {
                var description = Guid.NewGuid().ToString() + " {0}";

                transformation = transformation with
                {
                    Fields = new List<string>( fixture.CreateMany<string>( 3 ) ),
                    ViolationDescription = field => string.Format( description, field ),
                    AllowBlank = false,
                };

                var expectedEvents = new List<LogEvent>();
                var expectedData = new Dictionary<string, object>( record.Data );

                foreach ( var field in transformation.Fields )
                {
                    // we expect the transformation to remove the field from the data set
                    expectedData.Remove( field );
                    record.Data[field] = value;
                    expectedEvents.Add( new LogEvent
                    {
                        IsFatal = transformation.ViolationIsFatal,
                        Level = transformation.ViolationLogLevel,
                        Description = transformation.ViolationDescription( field ),
                    } );
                }

                await method();

                record.Data.Should().BeEquivalentTo( expectedData );
                record.Events.Should().BeEquivalentTo( expectedEvents );
            }

            [Theory, ClassData( typeof( Cases.WhiteSpaceCases ) )]
            public async Task retains_value_when_field_value_is_blank_and_allowed( string value )
            {
                transformation = transformation with
                {
                    Fields = new List<string>( fixture.CreateMany<string>( 3 ) ),
                    AllowBlank = true,
                };

                foreach ( var field in transformation.Fields )
                {
                    record.Data[field] = value;
                }

                var expectedData = new Dictionary<string, object>( record.Data );
                await method();

                record.Data.Should().BeEquivalentTo( expectedData );
                record.Events.Should().BeEmpty();
            }
        }
    }
}
