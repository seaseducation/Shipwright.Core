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

namespace Shipwright.Commands.Internal
{
    public class ValidationDecoratorTests
    {
        private ICommandHandler<FakeGuidCommand, Guid> inner;
        private IValidationAdapter<FakeGuidCommand> validator;
        private ICommandHandler<FakeGuidCommand, Guid> instance() => new ValidationDecorator<FakeGuidCommand, Guid>( inner, validator );
        private readonly Mock<ICommandHandler<FakeGuidCommand, Guid>> mockInner;
        private readonly Mock<IValidationAdapter<FakeGuidCommand>> mockValidator;

        public ValidationDecoratorTests()
        {
            mockInner = Mockery.Of( out inner );
            mockValidator = Mockery.Of( out validator );
        }

        public class Constructor : ValidationDecoratorTests
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

        public class Execute : ValidationDecoratorTests
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

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task rethrows_when_validation_fails( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                var expected = new ValidationException( Guid.NewGuid().ToString() );
                mockValidator.Setup( _ => _.ValidateAndThrow( command, cancellationToken ) ).ThrowsAsync( expected );
                var actual = await Assert.ThrowsAsync<ValidationException>( method );
                Assert.Equal( expected, actual );
            }

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task awaits_inner_handler_when_validation_passes( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var expected = Guid.NewGuid();
                mockValidator.Setup( _ => _.ValidateAndThrow( command, cancellationToken ) ).Returns( Task.CompletedTask );
                mockInner.Setup( _ => _.Execute( command, cancellationToken ) ).ReturnsAsync( expected );

                var actual = await method();
                Assert.Equal( expected, actual );

                mockValidator.Verify( _ => _.ValidateAndThrow( command, cancellationToken ), Times.Once() );
            }
        }
    }
}
