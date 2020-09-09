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
    public class CommandDispatcherTests
    {
        private IServiceProvider serviceProvider;
        private ICommandDispatcher instance() => new CommandDispatcher( serviceProvider );

        private readonly Mock<IServiceProvider> mockServiceProvider;

        public CommandDispatcherTests()
        {
            mockServiceProvider = Mockery.Of( out serviceProvider );
        }

        public class Constructor : CommandDispatcherTests
        {
            [Fact]
            public void requires_serviceProvider()
            {
                serviceProvider = null!;
                Assert.Throws<ArgumentNullException>( nameof( serviceProvider ), instance );
            }
        }

        public class Execute_Returning : CommandDispatcherTests
        {
            private Command<Guid> command = new FakeGuidCommand();
            private CancellationToken cancellationToken;
            private Task<Guid> method() => instance().Execute( command, cancellationToken );

            [Fact]
            public async Task requires_command()
            {
                command = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( command ), method );
            }

            [Fact]
            public async Task throws_when_no_handler_found()
            {
                var handlerType = typeof( ICommandHandler<FakeGuidCommand, Guid> );
                mockServiceProvider.Setup( _ => _.GetService( handlerType ) ).Returns( null );

                var ex = await Assert.ThrowsAsync<InvalidOperationException>( method );
                Assert.Equal( string.Format( Resources.CoreErrorMessages.MissingRequiredImplementation, handlerType ), ex.Message );
            }

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_awaited_result_from_inner_handler( bool canceled )
            {
                var expected = Guid.NewGuid();
                cancellationToken = new CancellationToken( canceled );
                var handlerType = typeof( ICommandHandler<FakeGuidCommand, Guid> );
                var mockHandler = Mockery.Of( out ICommandHandler<FakeGuidCommand, Guid> handler );
                mockServiceProvider.Setup( _ => _.GetService( handlerType ) ).Returns( handler );
                mockHandler.Setup( _ => _.Execute( (FakeGuidCommand)command, cancellationToken ) ).ReturnsAsync( expected );

                var actual = await method();
                Assert.Equal( expected, actual );
            }
        }

        public class Execute_Void : CommandDispatcherTests
        {
            private Command command = new FakeVoidCommand();
            private CancellationToken cancellationToken;

            private Task method() => instance().Execute( command, cancellationToken );

            [Fact]
            public async Task requires_command()
            {
                command = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( command ), method );
            }

            [Fact]
            public async Task throws_when_no_handler_found()
            {
                var handlerType = typeof( ICommandHandler<FakeVoidCommand, ValueTuple> );
                mockServiceProvider.Setup( _ => _.GetService( handlerType ) ).Returns( null );

                var ex = await Assert.ThrowsAsync<InvalidOperationException>( method );
                Assert.Equal( string.Format( Resources.CoreErrorMessages.MissingRequiredImplementation, handlerType ), ex.Message );
            }

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_awaited_result_from_inner_handler( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                var handlerType = typeof( ICommandHandler<FakeVoidCommand, ValueTuple> );
                var mockHandler = Mockery.Of( out ICommandHandler<FakeVoidCommand, ValueTuple> handler );
                mockServiceProvider.Setup( _ => _.GetService( handlerType ) ).Returns( handler );
                mockHandler.Setup( _ => _.Execute( (FakeVoidCommand)command, cancellationToken ) ).ReturnsAsync( default( ValueTuple ) );

                await method();
                mockHandler.Verify( _ => _.Execute( (FakeVoidCommand)command, cancellationToken ), Times.Once() );
            }
        }
    }
}
