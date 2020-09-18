// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Dataflows.Transformations.Internal
{
    public class CancellationFactoryDecoratorTests
    {
        private ITransformationFactory<FakeTransformation> inner;

        private ITransformationFactory<FakeTransformation> instance() => new CancellationFactoryDecorator<FakeTransformation>( inner );

        private readonly Mock<ITransformationFactory<FakeTransformation>> mockInner;

        public CancellationFactoryDecoratorTests()
        {
            mockInner = Mockery.Of( out inner );
        }

        public class Constructor : CancellationFactoryDecoratorTests
        {
            [Fact]
            public void requires_inner()
            {
                inner = null!;
                Assert.Throws<ArgumentNullException>( nameof( inner ), instance );
            }
        }

        public class Create : CancellationFactoryDecoratorTests
        {
            private FakeTransformation transformation = new FakeTransformation();
            private CancellationToken cancellationToken;

            private Task<ITransformationHandler> method() => instance().Create( transformation, cancellationToken );

            [Fact]
            public async Task requires_transformation()
            {
                transformation = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( transformation ), method );
            }

            [Fact]
            public async Task throws_when_canceled()
            {
                cancellationToken = new CancellationToken( true );
                await Assert.ThrowsAsync<OperationCanceledException>( method );
            }

            [Fact]
            public async Task decorates_handler_from_inner_factory()
            {
                cancellationToken = new CancellationToken( false );

                var handler = Mockery.Of<ITransformationHandler>();
                mockInner.Setup( _ => _.Create( transformation, cancellationToken ) ).ReturnsAsync( handler );

                var actual = await method();
                var decorated = Assert.IsType<CancellationHandlerDecorator>( actual );
                Assert.Same( handler, decorated.inner );
            }
        }
    }
}
