// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Databases.Internal
{
    public class DbConnectionDispatcherTests
    {
        private IServiceProvider serviceProvider;
        private IDbConnectionDispatcher instance() => new DbConnectionDispatcher( serviceProvider );

        private readonly Mock<IServiceProvider> mockServiceProvider;

        public DbConnectionDispatcherTests()
        {
            mockServiceProvider = Mockery.Of( out serviceProvider );
        }

        public class Constructor : DbConnectionDispatcherTests
        {
            [Fact]
            public void requires_serviceProvider()
            {
                serviceProvider = null!;
                Assert.Throws<ArgumentNullException>( nameof( serviceProvider ), instance );
            }
        }

        public class Build : DbConnectionDispatcherTests
        {
            private DbConnectionInfo connectionInfo = new FakeConnectionInfo();
            private CancellationToken cancellationToken;
            private Task<IDbConnectionFactory> method() => instance().Build( connectionInfo, cancellationToken );

            [Fact]
            public async Task requires_command()
            {
                connectionInfo = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( connectionInfo ), method );
            }

            [Fact]
            public async Task throws_when_no_builder_found()
            {
                var builderType = typeof( IDbConnectionBuilder<FakeConnectionInfo> );
                mockServiceProvider.Setup( _ => _.GetService( builderType ) ).Returns( null );

                var ex = await Assert.ThrowsAsync<InvalidOperationException>( method );
                Assert.Equal( string.Format( Resources.CoreErrorMessages.MissingRequiredImplementation, builderType ), ex.Message );
            }

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_awaited_result_from_inner_handler( bool canceled )
            {
                var expected = Mockery.Of<IDbConnectionFactory>();
                cancellationToken = new CancellationToken( canceled );
                var builderType = typeof( IDbConnectionBuilder<FakeConnectionInfo> );
                var mockBuilder = Mockery.Of( out IDbConnectionBuilder<FakeConnectionInfo> handler );
                mockServiceProvider.Setup( _ => _.GetService( builderType ) ).Returns( handler );
                mockBuilder.Setup( _ => _.Build( (FakeConnectionInfo)connectionInfo, cancellationToken ) ).ReturnsAsync( expected );

                var actual = await method();
                Assert.Same( expected, actual );
            }
        }
    }
}
