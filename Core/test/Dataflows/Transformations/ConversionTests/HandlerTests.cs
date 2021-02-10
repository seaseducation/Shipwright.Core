// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.Conversion;

namespace Shipwright.Dataflows.Transformations.ConversionTests
{
    public class HandlerTests
    {
        private Conversion transformation = new Conversion();
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

            private readonly Fixture fixture = new Fixture();

            [Fact]
            public async Task requires_record()
            {
                record = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( record ), method );
            }

            [Fact]
            public async Task noop_when_field_missing()
            {
                transformation = transformation with { Fields = fixture.CreateMany<string>().ToList() };

                foreach ( var field in transformation.Fields )
                {
                    record.Data.Remove( field );
                }

                var expected = new Dictionary<string, object>( record.Data );
                await method();

                Assert.Equal( expected, record.Data );
                Assert.Empty( record.Events );
            }

            [Fact]
            public async Task noop_when_field_null()
            {
                transformation = transformation with { Fields = fixture.CreateMany<string>().ToList() };

                foreach ( var field in transformation.Fields )
                {
                    record.Data[field] = null;
                }

                var expected = new Dictionary<string, object>( record.Data );
                await method();

                Assert.Equal( expected, record.Data );
                Assert.Empty( record.Events );
            }

            [Theory, Cases.BooleanCases]
            public async Task adds_event_when_coversion_fails( bool clearField )
            {
                bool fakeConverter( object value, out object result )
                {
                    result = null;
                    return false;
                }

                transformation = transformation with
                {
                    Fields = fixture.CreateMany<string>().ToList(),
                    Converter = fakeConverter,
                    ConversionFailedEvent = new FailureEventSetting { ClearField = clearField },
                };

                var expectedData = new Dictionary<string, object>( record.Data );
                var expectedEvents = new List<LogEvent>();

                foreach ( var field in transformation.Fields )
                {
                    record.Data[field] = fixture.Create<string>();

                    if ( !clearField )
                    {
                        expectedData[field] = record.Data[field];
                    }

                    expectedEvents.Add( new LogEvent
                    {
                        IsFatal = transformation.ConversionFailedEvent.IsFatal,
                        Level = transformation.ConversionFailedEvent.Level,
                        Description = transformation.ConversionFailedEvent.FailureEventMessage( field ),
                        Value = new { value = record.Data[field] },
                    } );
                }

                await method();

                Assert.Equal( expectedData, record.Data );
                Assert.Equal( JsonSerializer.Serialize( expectedEvents ), JsonSerializer.Serialize( record.Events ) );
            }

            [Fact]
            public async Task changes_field_value_when_conversion_succeeds()
            {
                var dictionary = new Dictionary<object, object>();

                bool fakeConverter( object value, out object result )
                {
                    result = dictionary[value];
                    return true;
                }

                transformation = transformation with
                {
                    Fields = fixture.CreateMany<string>().ToList(),
                    Converter = fakeConverter,
                };

                var expected = new Dictionary<string, object>( record.Data );

                foreach ( var field in transformation.Fields )
                {
                    record.Data[field] = fixture.Create<string>();
                    dictionary[record.Data[field]] = expected[field] = fixture.Create<string>();
                }

                await method();

                Assert.Equal( expected, record.Data );
                Assert.Empty( record.Events );
            }
        }
    }
}
