// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Moq;
using Shipwright.Databases;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.DbLookup;

namespace Shipwright.Dataflows.Transformations.DbLookupTests
{
    public class FactoryTests
    {
        private IDbConnectionDispatcher dispatcher;
        private ITransformationFactory<DbLookup> instance() => new Factory( dispatcher );

        private readonly Mock<IDbConnectionDispatcher> mockDispatcher;

        public FactoryTests()
        {
            mockDispatcher = Mockery.Of( out dispatcher );
        }

        public class Constructor : FactoryTests
        {
            [Fact]
            public void requires_dispatcher()
            {
                dispatcher = null!;
                Assert.Throws<ArgumentNullException>( nameof( dispatcher ), instance );
            }
        }

        public class Create : FactoryTests
        {
            private DbLookup transformation = new DbLookup();
            private CancellationToken cancellationToken;
            private Task<ITransformationHandler> method() => instance().Create( transformation, cancellationToken );

            [Fact]
            public async Task requires_transformation()
            {
                transformation = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( transformation ), method );
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_default_handler_when_caching_disabled( bool canceled )
            {
                transformation = transformation with { CacheResults = false };
                cancellationToken = new CancellationToken( canceled );

                var connectionFactory = Mockery.Of<IDbConnectionFactory>();
                mockDispatcher.Setup( _ => _.Build( transformation.ConnectionInfo, cancellationToken ) ).ReturnsAsync( connectionFactory );

                var actual = await method();
                var typed = Assert.IsType<Handler>( actual );
                Assert.Same( transformation, typed.transformation );
                Assert.Same( connectionFactory, typed.connectionFactory );
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_caching_handler_when_caching_enabled( bool canceled )
            {
                transformation = transformation with { CacheResults = true };
                cancellationToken = new CancellationToken( canceled );

                var connectionFactory = Mockery.Of<IDbConnectionFactory>();
                mockDispatcher.Setup( _ => _.Build( transformation.ConnectionInfo, cancellationToken ) ).ReturnsAsync( connectionFactory );

                var actual = await method();
                var typed = Assert.IsType<CacheHandler>( actual );
                Assert.Same( transformation, typed.transformation );
                Assert.Same( connectionFactory, typed.connectionFactory );
            }
        }
    }
}
