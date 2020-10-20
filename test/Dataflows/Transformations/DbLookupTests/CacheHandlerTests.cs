// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using FluentAssertions;
using Moq;
using Shipwright.Databases;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.DbLookup;

namespace Shipwright.Dataflows.Transformations.DbLookupTests
{
    public class CacheHandlerTests
    {
        private DbLookup transformation = new DbLookup { CacheResults = true };
        private readonly IDbConnectionFactory connectionFactory = Mockery.Of<IDbConnectionFactory>();
        private CacheHandler instance() => mockHandler.Object;

        private Mock<CacheHandler> mockHandler;

        public CacheHandlerTests()
        {
            mockHandler = new Mock<CacheHandler>( MockBehavior.Loose, transformation, connectionFactory ) { CallBase = true };
        }

        public class GetMatches : CacheHandlerTests
        {
            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_same_result_on_subsequent_call_when_parameters_equal_and_caching_desired( bool canceled )
            {
                var fixture = new Fixture();
                var cancellationToken = new CancellationToken( canceled );
                var parameters = fixture.Create<Dictionary<string, string>>().ToDictionary( _ => _.Key, _ => (object)_.Value );
                var expected = fixture.CreateMany<string>( 3 );

                var capture = new List<IDictionary<string, object>>();
                mockHandler.Setup( _ => _.GetMatchesEx( Capture.In( capture ), cancellationToken ) ).ReturnsAsync( expected );

                var instance = base.instance();
                var first = await instance.GetMatches( parameters, cancellationToken );
                var second = await instance.GetMatches( new Dictionary<string, object>( parameters ), cancellationToken );

                Assert.Same( expected, first );
                Assert.Same( expected, second );
                capture.Should().ContainSingle().Subject.Should().BeSameAs( parameters );
                mockHandler.Verify( _ => _.GetMatchesEx( It.IsAny<IDictionary<string, object>>(), cancellationToken ), Times.Once() );
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_different_result_on_subsequent_call_when_parameters_equal_and_caching_desired( bool canceled )
            {
                var fixture = new Fixture();
                var cancellationToken = new CancellationToken( canceled );
                var parameters1 = fixture.Create<Dictionary<string, string>>().ToDictionary( _ => _.Key, _ => (object)_.Value.ToLowerInvariant() );
                var parameters2 = parameters1.ToDictionary( _ => _.Key, _ => (object)_.Value.ToString().ToUpperInvariant() );
                var expected1 = fixture.CreateMany<string>( 3 );
                var expected2 = fixture.CreateMany<string>( 3 );

                mockHandler.Setup( _ => _.GetMatchesEx( parameters1, cancellationToken ) ).ReturnsAsync( expected1 );
                mockHandler.Setup( _ => _.GetMatchesEx( parameters2, cancellationToken ) ).ReturnsAsync( expected2 );

                var instance = base.instance();
                var first = await instance.GetMatches( parameters1, cancellationToken );
                var second = await instance.GetMatches( parameters2, cancellationToken );

                Assert.Same( expected1, first );
                Assert.Same( expected2, second );
                mockHandler.Verify( _ => _.GetMatchesEx( It.IsAny<IDictionary<string, object>>(), cancellationToken ), Times.Exactly( 2 ) );
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_same_result_on_subsequent_call_when_parameters_equal_and_caching_not_desired( bool canceled )
            {
                // note: must reset the mock to change transformation values
                transformation = transformation with { CacheResults = false };
                mockHandler = new Mock<CacheHandler>( MockBehavior.Loose, transformation, connectionFactory ) { CallBase = true };

                var fixture = new Fixture();
                var cancellationToken = new CancellationToken( canceled );
                var parameters = fixture.Create<Dictionary<string, string>>().ToDictionary( _ => _.Key, _ => (object)_.Value );
                var expected = new[] { fixture.CreateMany<string>( 3 ), fixture.CreateMany<string>( 3 ) };

                var index = 0;
                mockHandler.Setup( _ => _.GetMatchesEx( parameters, cancellationToken ) ).ReturnsAsync( () => expected[index++] );

                var instance = base.instance();
                var first = await instance.GetMatches( parameters, cancellationToken );
                var second = await instance.GetMatches( parameters, cancellationToken );

                Assert.Same( expected[0], first );
                Assert.Same( expected[1], second );
                mockHandler.Verify( _ => _.GetMatchesEx( parameters, cancellationToken ), Times.Exactly( 2 ) );
            }
        }
    }
}
