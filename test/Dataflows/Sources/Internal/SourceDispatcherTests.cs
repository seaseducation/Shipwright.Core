// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using Moq;
using Shipwright.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Dataflows.Sources.Internal
{
    public class SourceDispatcherTests
    {
        private IServiceProvider serviceProvider;

        private ISourceDispatcher instance() => new SourceDispatcher( serviceProvider );

        private readonly Mock<IServiceProvider> mockServiceProvider;

        public SourceDispatcherTests()
        {
            mockServiceProvider = Mockery.Of( out serviceProvider );
        }

        public class Constructor : SourceDispatcherTests
        {
            [Fact]
            public void requires_serviceProvider()
            {
                serviceProvider = null!;
                Assert.Throws<ArgumentNullException>( nameof( serviceProvider ), instance );
            }
        }

        public class Read : SourceDispatcherTests
        {
            private Source source = new FakeSource();
            private StringComparer comparer;
            private CancellationToken cancellationToken;

            private async Task<List<Record>> method() => await instance().Read( source, comparer, cancellationToken ).ToListAsync();

            [Fact]
            public async Task requires_source()
            {
                source = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( source ), method );
            }

            [Fact]
            public async Task throws_when_handler_not_found()
            {
                mockServiceProvider.Setup( _ => _.GetService( typeof( ISourceHandler<FakeSource> ) ) ).Returns( null ).Verifiable();

                var actual = await Assert.ThrowsAsync<InvalidOperationException>( method );
                Assert.Equal( string.Format( CoreErrorMessages.MissingRequiredImplementation, typeof( ISourceHandler<FakeSource> ) ), actual.Message );
            }

            public class ArgumentCases : TheoryData<StringComparer, bool>
            {
                public ArgumentCases()
                {
                    var comparers = new[] { StringComparer.Ordinal, StringComparer.OrdinalIgnoreCase };
                    var booealns = new[] { true, false };

                    foreach ( var comparer in comparers )
                    {
                        foreach ( var canceled in booealns )
                        {
                            Add( comparer, canceled );
                        }
                    }
                }
            }

            [Theory, ClassData( typeof( ArgumentCases ) )]
            public async Task returns_records( StringComparer comparer, bool canceled )
            {
                this.comparer = comparer;
                cancellationToken = new CancellationToken( canceled );

                var mockHandler = Mockery.Of( out ISourceHandler<FakeSource> handler );
                mockServiceProvider.Setup( _ => _.GetService( typeof( ISourceHandler<FakeSource> ) ) ).Returns( handler );

                var fixture = new Fixture();
                var expected = Enumerable.Range( 0, 3 ).Select( position => new Record( source, fixture.Create<IDictionary<string, object>>(), position, comparer ) ).ToArray();

                async IAsyncEnumerable<Record> callback()
                {
                    foreach ( var record in expected )
                    {
                        yield return record;
                    }
                }

                mockHandler.Setup( _ => _.Read( (FakeSource)source, comparer, cancellationToken ) ).Returns( callback );

                var actual = await method();
                Assert.Equal( expected, actual );
            }
        }
    }
}
