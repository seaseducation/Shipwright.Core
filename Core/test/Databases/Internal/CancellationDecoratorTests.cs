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
    public class CancellationDecoratorTests
    {
        private IDbConnectionBuilder<FakeConnectionInfo> inner;
        private IDbConnectionBuilder<FakeConnectionInfo> instance() => new CancellationDecorator<FakeConnectionInfo>( inner );
        private readonly Mock<IDbConnectionBuilder<FakeConnectionInfo>> mockInner;

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

        public class Execute : CancellationDecoratorTests
        {
            private FakeConnectionInfo connectionInfo = new FakeConnectionInfo();
            private CancellationToken cancellationToken;
            private Task<IDbConnectionFactory> method() => instance().Build( connectionInfo, cancellationToken );

            [Fact]
            public async Task requires_connectionInfo()
            {
                connectionInfo = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( connectionInfo ), method );
            }

            [Fact]
            public async Task throws_when_canceled()
            {
                cancellationToken = new CancellationToken( true );
                await Assert.ThrowsAsync<OperationCanceledException>( method );
            }

            [Fact]
            public async Task awaits_inner_builder_when_not_canceled()
            {
                cancellationToken = new CancellationToken( false );

                var expected = Mockery.Of<IDbConnectionFactory>();
                mockInner.Setup( _ => _.Build( connectionInfo, cancellationToken ) ).ReturnsAsync( expected );

                var actual = await method();
                Assert.Same( expected, actual );
            }
        }
    }
}
