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
        public class Context<TTransformation> where TTransformation : Transformation
        {
            public ITransformationFactory<TTransformation> inner;
            public ITransformationFactory<TTransformation> instance() => new ThrottleableFactoryDecorator<TTransformation>( inner );

            public Mock<ITransformationFactory<TTransformation>> mockInner;

            public Context()
            {
                mockInner = Mockery.Of( out inner );
            }
        }

        public record FakeThrottleableTransformation : ThrottleableTransformation
        {
            public Guid Value { get; init; } = Guid.NewGuid();
        }

        public abstract class Constructor<TTransformation> : Context<TTransformation> where TTransformation : Transformation
        {
            [Fact]
            public void requires_inner()
            {
                inner = null!;
                Assert.Throws<ArgumentNullException>( nameof( inner ), instance );
            }
        }

        public class Constructor_Normal : Constructor<FakeTransformation> { }
        public class Constructor_Throttleable : Constructor<FakeThrottleableTransformation> { }

        public abstract class Create<TTransformation> : Context<TTransformation> where TTransformation : Transformation, new()
        {
            public TTransformation transformation = new TTransformation();
            public CancellationToken cancellationToken;
            public Task<ITransformationHandler> method() => instance().Create( transformation, cancellationToken );

            [Fact]
            public async Task requires_transformation()
            {
                transformation = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( transformation ), method );
            }
        }

        public class Create_Normal : Create<FakeTransformation>
        {
            [Theory, Cases.BooleanCases]
            public async Task returns_handler_from_inner( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var expected = Mockery.Of<ITransformationHandler>();
                mockInner.Setup( _ => _.Create( transformation, cancellationToken ) ).ReturnsAsync( expected ).Verifiable();

                var actual = await method();
                Assert.Same( expected, actual );
            }
        }

        public class Create_Throttleable : Create<FakeThrottleableTransformation>
        {
            [Theory, Cases.BooleanCases]
            public async Task returns_handler_from_inner_when_maxdop_is_unlimited( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                transformation = new FakeThrottleableTransformation { MaxDegreeOfParallelism = int.MaxValue };

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
                transformation = new FakeThrottleableTransformation { MaxDegreeOfParallelism = maxDop };

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
