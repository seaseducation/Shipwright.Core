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
    public class EventInspectionFactoryDecoratorTests
    {
        private ITransformationFactory<FakeTransformation> inner;
        private ITransformationFactory<FakeTransformation> instance() => new EventInspectionFactoryDecorator<FakeTransformation>( inner );

        private readonly Mock<ITransformationFactory<FakeTransformation>> mockInner;

        public EventInspectionFactoryDecoratorTests()
        {
            mockInner = Mockery.Of( out inner );
        }

        public class Constructor : EventInspectionFactoryDecoratorTests
        {
            [Fact]
            public void requires_inner()
            {
                inner = null!;
                Assert.Throws<ArgumentNullException>( nameof( inner ), instance );
            }
        }

        public class Create : EventInspectionFactoryDecoratorTests
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

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task decorates_handler_from_inner_factory( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var handler = Mockery.Of<ITransformationHandler>();
                mockInner.Setup( _ => _.Create( transformation, cancellationToken ) ).ReturnsAsync( handler );

                var actual = await method();
                var decorated = Assert.IsType<EventInspectionHandlerDecorator>( actual );
                Assert.Same( handler, decorated.inner );
            }
        }
    }
}
