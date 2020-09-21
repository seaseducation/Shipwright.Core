// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using AutoFixture.Xunit2;
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
            private StringComparer comparer;
            private CancellationToken cancellationToken;

            private async Task<List<Record>> method() => await instance().Read( source, comparer, cancellationToken ).ToListAsync();

            [Fact]
            public async Task requires_source()
            {
                source = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( source ), method );
            }

            [Theory, AutoData]
            public async Task throws_when_canceled_early( StringComparer comparer )
            {
                this.comparer = comparer;
                cancellationToken = new CancellationToken( true );

                await Assert.ThrowsAsync<OperationCanceledException>( method );
            }

            [Theory, AutoData]
            public async Task throws_when_canceled_late( StringComparer comparer )
            {
                this.comparer = comparer;

                using var cts = new CancellationTokenSource();
                cancellationToken = cts.Token;

                var fixture = new Fixture();
                var records = Enumerable.Range( 0, 3 ).Select( position => new Record( source, fixture.Create<IDictionary<string, object>>(), position, comparer ) ).ToArray();

                async IAsyncEnumerable<Record> callback()
                {
                    foreach ( var record in records )
                    {
                        yield return record;
                        cts.Cancel();
                    }
                }

                mockInner.Setup( _ => _.Read( source, comparer, cancellationToken ) ).Returns( callback );
                await Assert.ThrowsAsync<OperationCanceledException>( method );
            }

            [Theory, AutoData]
            public async Task returns_records( StringComparer comparer )
            {
                this.comparer = comparer;
                cancellationToken = new CancellationToken( false );

                var fixture = new Fixture();
                var expected = Enumerable.Range( 0, 3 ).Select( position => new Record( source, fixture.Create<IDictionary<string, object>>(), position, comparer ) ).ToArray();

                async IAsyncEnumerable<Record> callback()
                {
                    foreach ( var record in expected )
                    {
                        yield return record;
                    }
                }

                mockInner.Setup( _ => _.Read( source, comparer, cancellationToken ) ).Returns( callback );

                var actual = await method();
                Assert.Equal( expected, actual );
            }
        }
    }
}
