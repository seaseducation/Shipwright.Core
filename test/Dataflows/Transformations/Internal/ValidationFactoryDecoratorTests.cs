// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using Moq;
using Shipwright.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Dataflows.Transformations.Internal
{
    public class ValidationFactoryDecoratorTests
    {
        private ITransformationFactory<FakeTransformation> inner;
        private IValidationAdapter<FakeTransformation> validator;
        private ITransformationFactory<FakeTransformation> instance() => new ValidationFactoryDecorator<FakeTransformation>( inner, validator );

        private readonly Mock<ITransformationFactory<FakeTransformation>> mockInner;
        private readonly Mock<IValidationAdapter<FakeTransformation>> mockValidator;

        public ValidationFactoryDecoratorTests()
        {
            mockInner = Mockery.Of( out inner );
            mockValidator = Mockery.Of( out validator );
        }

        public class Constructor : ValidationFactoryDecoratorTests
        {
            [Fact]
            public void requires_inner()
            {
                inner = null!;
                Assert.Throws<ArgumentNullException>( nameof( inner ), instance );
            }

            [Fact]
            public void requires_validator()
            {
                validator = null!;
                Assert.Throws<ArgumentNullException>( nameof( validator ), instance );
            }
        }

        public class Create : ValidationFactoryDecoratorTests
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

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task rethrows_exception_from_validator( bool canceled )
            {
                var expected = new ValidationException( Guid.NewGuid().ToString() );
                cancellationToken = new CancellationToken( canceled );
                mockValidator.Setup( _ => _.ValidateAndThrow( transformation, cancellationToken ) ).ThrowsAsync( expected );

                var actual = await Assert.ThrowsAsync<ValidationException>( method );
                Assert.Same( expected, actual );
            }

            [Theory, ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_awaited_handler_from_inner_factory( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var expected = Mockery.Of<ITransformationHandler>();
                mockValidator.Setup( _ => _.ValidateAndThrow( transformation, cancellationToken ) ).Returns( Task.CompletedTask );
                mockInner.Setup( _ => _.Create( transformation, cancellationToken ) ).ReturnsAsync( expected );

                var actual = await method();
                Assert.Same( expected, actual );
            }
        }
    }
}
