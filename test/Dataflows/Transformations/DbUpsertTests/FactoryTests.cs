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
using static Shipwright.Dataflows.Transformations.DbUpsert;

namespace Shipwright.Dataflows.Transformations.DbUpsertTests
{
    public class FactoryTests
    {
        private IDbConnectionDispatcher connectionDispatcher;
        private ITransformationFactory<DbUpsert> instance() => new Factory( connectionDispatcher );

        private readonly Mock<IDbConnectionDispatcher> mockConnectionDispatcher;

        public FactoryTests()
        {
            mockConnectionDispatcher = Mockery.Of( out connectionDispatcher );
        }

        public class Constructor : FactoryTests
        {
            [Fact]
            public void requires_connectionDispatcher()
            {
                connectionDispatcher = null!;
                Assert.Throws<ArgumentNullException>( nameof( connectionDispatcher ), instance );
            }
        }

        public class Create : FactoryTests
        {
            private DbUpsert transformation = new DbUpsert { ConnectionInfo = new FakeConnectionInfo() };
            private CancellationToken cancellationToken;
            private Task<ITransformationHandler> method() => instance().Create( transformation, cancellationToken );

            [Fact]
            public async Task requires_transformation()
            {
                transformation = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( transformation ), method );
            }

            [Theory, Cases.BooleanCases]
            public async Task returns_configured_handler( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var connectionFactory = Mockery.Of<IDbConnectionFactory>();
                mockConnectionDispatcher.Setup( _ => _.Build( transformation.ConnectionInfo, cancellationToken ) ).ReturnsAsync( connectionFactory );

                var actual = await method();
                var typed = Assert.IsType<Handler>( actual );
                Assert.Same( transformation, typed.transformation );
                Assert.Same( connectionFactory, typed.connectionFactory );
            }
        }
    }
}
