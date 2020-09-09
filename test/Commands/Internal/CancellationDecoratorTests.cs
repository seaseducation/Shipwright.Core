// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Commands.Internal
{
    public class CancellationDecoratorTests
    {
        private ICommandHandler<FakeGuidCommand, Guid> inner;
        private ICommandHandler<FakeGuidCommand, Guid> instance() => new CancellationDecorator<FakeGuidCommand, Guid>( inner );
        private readonly Mock<ICommandHandler<FakeGuidCommand, Guid>> mockInner;

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
            private FakeGuidCommand command = new FakeGuidCommand();
            private CancellationToken cancellationToken;
            private Task<Guid> method() => instance().Execute( command, cancellationToken );

            [Fact]
            public async Task requires_command()
            {
                command = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( command ), method );
            }

            [Fact]
            public async Task throws_when_canceled()
            {
                cancellationToken = new CancellationToken( true );
                await Assert.ThrowsAsync<OperationCanceledException>( method );
            }

            [Fact]
            public async Task awaits_inner_handler_when_not_canceled()
            {
                cancellationToken = new CancellationToken( false );

                var expected = Guid.NewGuid();
                mockInner.Setup( _ => _.Execute( command, cancellationToken ) ).ReturnsAsync( expected );

                var actual = await method();
                Assert.Equal( expected, actual );
            }
        }
    }
}
