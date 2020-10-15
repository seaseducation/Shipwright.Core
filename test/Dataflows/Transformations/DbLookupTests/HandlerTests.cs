// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using FluentAssertions;
using Moq;
using Shipwright.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.DbLookup;

namespace Shipwright.Dataflows.Transformations.DbLookupTests
{
    public class HandlerTests
    {
        private DbLookup transformation = new DbLookup();
        private IDbConnectionFactory connectionFactory = Mockery.Of<IDbConnectionFactory>();
        private ITransformationHandler instance() => new Handler( transformation, connectionFactory );

        public class Constructor : HandlerTests
        {
            [Fact]
            public void requires_transformation()
            {
                transformation = null!;
                Assert.Throws<ArgumentNullException>( nameof( transformation ), instance );
            }

            [Fact]
            public void requires_connectionFactory()
            {
                connectionFactory = null!;
                Assert.Throws<ArgumentNullException>( nameof( connectionFactory ), instance );
            }
        }

        public class MapInputs : HandlerTests
        {
            private readonly Record record = FakeRecord.Create();
            private IDictionary<string, object> method() => (instance() as Handler).MapInputs( record );

            [Fact]
            public void returns_dictionary_with_values_for_all_input_parameters()
            {
                transformation.Input.Clear();

                var expected = new Dictionary<string, object>();
                var fixture = new Fixture();

                // add parameter with a defined field value
                var first = fixture.Create<(string field, string parameter)>();
                record.Data[first.field] = expected[first.parameter] = Guid.NewGuid();
                transformation.Input.Add( first );

                // add parameter with a null field value
                var second = fixture.Create<(string field, string parameter)>();
                record.Data[second.field] = expected[second.parameter] = null;
                transformation.Input.Add( second );

                // add parameter with a missing field value
                var third = fixture.Create<(string field, string parameter)>();
                record.Data.Remove( third.field );
                expected[third.parameter] = null;
                transformation.Input.Add( third );

                var actual = method();
                actual.Should().BeEquivalentTo( expected );
            }
        }

        public class TryGetResult : HandlerTests
        {
            private readonly Record record = FakeRecord.Create();
            private IEnumerable<dynamic> matches;
            private object parameters;
            private IDictionary<string, object> result;
            private bool method() => (instance() as Handler).TryGetResult( record, matches, parameters, out result );

            private readonly Fixture fixture = new Fixture();

            [Fact]
            public void returns_false_and_adds_event_when_no_matches()
            {
                matches = Array.Empty<dynamic>();
                parameters = fixture.Create<object>();

                var actual = method();
                Assert.False( actual );
                Assert.Null( result );

                var settings = transformation.QueryZeroRecordEvent;
                var expected = new LogEvent
                {
                    IsFatal = settings.IsFatal,
                    Level = settings.Level,
                    Description = settings.FailureEventMessage( 0 ),
                    Value = parameters
                };

                record.Events.Should().ContainSingle().Subject.Should().BeEquivalentTo( expected );
            }

            [Fact]
            public void returns_false_and_adds_event_when_multiple_matches()
            {
                matches = fixture.CreateMany<Dictionary<string, object>>( 2 );
                parameters = fixture.Create<object>();

                var actual = method();
                Assert.False( actual );
                Assert.Same( matches.First(), result );

                var settings = transformation.QueryMultipleRecordEvent;
                var expected = new LogEvent
                {
                    IsFatal = settings.IsFatal,
                    Level = settings.Level,
                    Description = settings.FailureEventMessage( 2 ),
                    Value = parameters
                };

                record.Events.Should().ContainSingle().Subject.Should().BeEquivalentTo( expected );
            }

            [Fact]
            public void returns_true_when_single_match()
            {
                matches = fixture.CreateMany<Dictionary<string, object>>( 1 );
                parameters = fixture.Create<object>();

                var actual = method();
                Assert.True( actual );
                Assert.Same( matches.Single(), result );

                record.Events.Should().BeEmpty();
            }
        }

        public class MapResult : HandlerTests
        {
            private readonly Record record = FakeRecord.Create();
            private readonly IDictionary<string, object> result = new Dictionary<string, object>();

            private void method() => (instance() as Handler).MapResult( record, result );

            [Fact]
            public void overrites_existing_field_when_column_present()
            {
                var fixture = new Fixture();
                transformation = transformation with { Output = new List<(string, string)>( fixture.CreateMany<(string, string)>( 3 ) ) };

                var expected = new Dictionary<string, object>( record.Data );

                foreach ( var (field, column) in transformation.Output )
                {
                    record.Data[field] = Guid.NewGuid();
                    expected[field] = result[column] = Guid.NewGuid();
                }

                method();
                record.Data.Should().BeEquivalentTo( expected );
            }

            [Fact]
            public void creates_field_when_column_present()
            {
                var fixture = new Fixture();
                transformation = transformation with { Output = new List<(string, string)>( fixture.CreateMany<(string, string)>( 3 ) ) };

                var expected = new Dictionary<string, object>( record.Data );

                foreach ( var (field, column) in transformation.Output )
                {
                    record.Data.Remove( field );
                    expected[field] = result[column] = Guid.NewGuid();
                }

                method();
                record.Data.Should().BeEquivalentTo( expected );
            }

            [Fact]
            public void ignores_field_when_column_missing()
            {
                var fixture = new Fixture();
                transformation = transformation with { Output = new List<(string, string)>( fixture.CreateMany<(string, string)>( 3 ) ) };

                var expected = new Dictionary<string, object>( record.Data );

                foreach ( var (field, column) in transformation.Output )
                {
                    expected[field] = record.Data[field] = Guid.NewGuid();
                    result.Remove( column );
                }

                method();
                record.Data.Should().BeEquivalentTo( expected );
            }
        }

        public class Transform : HandlerTests
        {
            private new ITransformationHandler instance() => mockHandler.Object;

            private readonly Mock<Handler> mockHandler;
            private Record record = FakeRecord.Create();
            private CancellationToken cancellationToken;

            private Task method() => instance().Transform( record, cancellationToken );

            public Transform()
            {
                mockHandler = new Mock<Handler>( MockBehavior.Loose, transformation, connectionFactory ) { CallBase = true };
            }

            [Fact]
            public async Task requires_record()
            {
                record = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( record ), method );
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task maps_query_results_when_matches_found( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var fixture = new Fixture();
                var parameters = new Dictionary<string, object>();
                var matches = fixture.CreateMany<IDictionary<string, object>>( 1 );
                var result = fixture.Create<IDictionary<string, object>>();

                var sequence = new MockSequence();
                mockHandler.InSequence( sequence ).Setup( _ => _.MapInputs( record ) ).Returns( parameters );
                mockHandler.InSequence( sequence ).Setup( _ => _.GetMatches( parameters, cancellationToken ) ).ReturnsAsync( matches );
                mockHandler.InSequence( sequence ).Setup( _ => _.TryGetResult( record, matches, parameters, out result ) ).Returns( true );
                mockHandler.InSequence( sequence ).Setup( _ => _.MapResult( record, result ) );

                await method();
                mockHandler.Verify( _ => _.MapResult( record, result ), Times.Once() );
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task noop_when_result_not_matched( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var fixture = new Fixture();
                var parameters = new Dictionary<string, object>();
                var matches = fixture.CreateMany<IDictionary<string, object>>( 3 );
                var result = fixture.Create<IDictionary<string, object>>();

                var sequence = new MockSequence();
                mockHandler.InSequence( sequence ).Setup( _ => _.MapInputs( record ) ).Returns( parameters );
                mockHandler.InSequence( sequence ).Setup( _ => _.GetMatches( parameters, cancellationToken ) ).ReturnsAsync( matches );
                mockHandler.InSequence( sequence ).Setup( _ => _.TryGetResult( record, matches, parameters, out result ) ).Returns( false );

                await method();
                mockHandler.Verify( _ => _.TryGetResult( record, matches, parameters, out result ), Times.Once() );
                mockHandler.Verify( _ => _.MapResult( record, result ), Times.Never() );
            }
        }
    }
}
