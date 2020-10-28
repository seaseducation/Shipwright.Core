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
    public class ThrottleableFactoryDecoratorTests
    {
        private ITransformationFactory<FakeTransformation> inner;
        private ITransformationFactory<FakeTransformation> instance() => new ThrottleableFactoryDecorator<FakeTransformation>( inner );

        private readonly Mock<ITransformationFactory<FakeTransformation>> mockInner;

        public ThrottleableFactoryDecoratorTests()
        {
            mockInner = Mockery.Of( out inner );
        }

        public class Constructor : ThrottleableFactoryDecoratorTests
        {
            [Fact]
            public void requires_inner()
            {
                inner = null!;
                Assert.Throws<ArgumentNullException>( nameof( inner ), instance );
            }
        }

        public class Create : ThrottleableFactoryDecoratorTests
        {
            private FakeTransformation transformation = new FakeTransformation();
            public CancellationToken cancellationToken;
            public Task<ITransformationHandler> method() => instance().Create( transformation, cancellationToken );

            [Fact]
            public async Task requires_transformation()
            {
                transformation = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( transformation ), method );
            }

            [Theory, Cases.BooleanCases]
            public async Task returns_handler_from_inner_when_maxdop_is_unlimited( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                transformation = new FakeTransformation { MaxDegreeOfParallelism = int.MaxValue };

                var expected = Mockery.Of<ITransformationHandler>();
                mockInner.Setup( _ => _.Create( transformation, cancellationToken ) ).ReturnsAsync( expected ).Verifiable();

                var actual = await method();
                Assert.Same( expected, actual );
            }

            [Theory, Cases.BooleanCases]
            public async Task returns_decorated_handler_when_maxdop_is_limited( bool canceled )
            {
                var random = new Random();
                var maxDop = 0;

                while ( maxDop <= 0 || maxDop == int.MaxValue )
                {
                    maxDop = random.Next();
                }

                cancellationToken = new CancellationToken( canceled );
                transformation = new FakeTransformation { MaxDegreeOfParallelism = maxDop };

                var expected = Mockery.Of<ITransformationHandler>();
                mockInner.Setup( _ => _.Create( transformation, cancellationToken ) ).ReturnsAsync( expected ).Verifiable();

                var actual = await method();
                var typed = Assert.IsType<ThrottleableHandlerDecorator>( actual );
                Assert.Same( expected, typed.inner );
                Assert.Equal( maxDop, typed.semaphore.CurrentCount );
            }
        }
    }
}
