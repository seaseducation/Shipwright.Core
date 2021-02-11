// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Sources.AggregateSource;

namespace Shipwright.Dataflows.Sources.AggregateSourceTests
{
    public class HandlerTests
    {
        private ISourceDispatcher sourceDispatcher;
        private ISourceHandler<AggregateSource> instance() => new Handler( sourceDispatcher );

        private readonly Mock<ISourceDispatcher> mockSourceDispatcher;

        public HandlerTests()
        {
            mockSourceDispatcher = Mockery.Of( out sourceDispatcher );
        }

        public class Constructor : HandlerTests
        {
            [Fact]
            public void requires_sourceDispatcher()
            {
                sourceDispatcher = null!;
                Assert.Throws<ArgumentNullException>( nameof( sourceDispatcher ), instance );
            }
        }

        public class Read : HandlerTests
        {
            private AggregateSource source = new AggregateSource { };
            private Dataflow dataflow = new Dataflow();
            private CancellationToken cancellationToken;

            private async Task<List<Record>> method() => await instance().Read( source, dataflow, cancellationToken ).ToListAsync();

            [Fact]
            public async Task requires_source()
            {
                source = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( source ), method );
            }

            [Fact]
            public async Task requires_dataflow()
            {
                dataflow = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( dataflow ), method );
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task reads_data_from_child_sources( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var fixture = FakeRecord.Fixture();
                var sources = fixture.CreateMany<FakeSource>( 3 );
                var expected = new List<Record>();

                foreach ( var source in sources )
                {
                    var records = fixture.CreateMany<Record>();
                    expected.AddRange( records );
                    this.source.Sources.Add( source );

                    mockSourceDispatcher.Setup( _ => _.Read( source, dataflow, cancellationToken ) ).Returns( records.ToAsyncEnumerable() );
                }

                var actual = await method();
                Assert.Equal( expected, actual );
            }
        }
    }
}
