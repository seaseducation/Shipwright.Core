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

namespace Shipwright.Dataflows.Sources.Internal
{
    public class CancellationDecoratorTests
    {
        private ISourceHandler<FakeSource> inner;
        private ISourceHandler<FakeSource> instance() => new CancellationDecorator<FakeSource>( inner );

        private readonly Mock<ISourceHandler<FakeSource>> mockInner;

        public CancellationDecoratorTests()
        {
            mockInner = Mockery.Of( out inner );
        }

        public class Constructor : CancellationDecoratorTests
        {
            [Fact]
            public void requires_inner()
            {
                inner = null!;
                Assert.Throws<ArgumentNullException>( nameof( inner ), instance );
            }
        }

        public class Read : CancellationDecoratorTests
        {
            private FakeSource source = new FakeSource();
            private Dataflow dataflow = FakeRecord.Fixture().Create<Dataflow>();
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

            [Fact]
            public async Task throws_when_canceled_early()
            {
                cancellationToken = new CancellationToken( true );
                await Assert.ThrowsAsync<OperationCanceledException>( method );
            }

            [Fact]
            public async Task throws_when_canceled_late()
            {
                using var cts = new CancellationTokenSource();
                cancellationToken = cts.Token;

                var fixture = new Fixture();
                fixture.Register( () => dataflow );
                fixture.Register<Source>( () => source );
                var records = fixture.CreateMany<Record>( 3 );

                async IAsyncEnumerable<Record> callback()
                {
                    foreach ( var record in records )
                    {
                        yield return record;
                        cts.Cancel();
                    }
                }

                mockInner.Setup( _ => _.Read( source, dataflow, cancellationToken ) ).Returns( callback );
                await Assert.ThrowsAsync<OperationCanceledException>( method );
            }

            [Fact]
            public async Task returns_records()
            {
                cancellationToken = new CancellationToken( false );

                var fixture = new Fixture();
                fixture.Register( () => dataflow );
                fixture.Register<Source>( () => source );
                var expected = fixture.CreateMany<Record>();

                async IAsyncEnumerable<Record> callback()
                {
                    foreach ( var record in expected )
                    {
                        yield return record;
                    }
                }

                mockInner.Setup( _ => _.Read( source, dataflow, cancellationToken ) ).Returns( callback );

                var actual = await method();
                Assert.Equal( expected, actual );
            }
        }
    }
}
