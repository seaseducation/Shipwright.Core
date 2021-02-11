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

namespace Shipwright.Databases.Internal
{
    public class ValidationDecoratorTests
    {
        private IDbConnectionBuilder<FakeConnectionInfo> inner;
        private IValidationAdapter<FakeConnectionInfo> validator;
        private IDbConnectionBuilder<FakeConnectionInfo> instance() => new ValidationDecorator<FakeConnectionInfo>( inner, validator );
        private readonly Mock<IDbConnectionBuilder<FakeConnectionInfo>> mockInner;
        private readonly Mock<IValidationAdapter<FakeConnectionInfo>> mockValidator;

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
            private FakeConnectionInfo connectionInfo = new FakeConnectionInfo();
            private CancellationToken cancellationToken;
            private Task<IDbConnectionFactory> method() => instance().Build( connectionInfo, cancellationToken );

            [Fact]
            public async Task requires_connectionInfo()
            {
                connectionInfo = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( connectionInfo ), method );
            }

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task rethrows_when_validation_fails( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                var expected = new ValidationException( Guid.NewGuid().ToString() );
                mockValidator.Setup( _ => _.ValidateAndThrow( connectionInfo, cancellationToken ) ).ThrowsAsync( expected );
                var actual = await Assert.ThrowsAsync<ValidationException>( method );
                Assert.Equal( expected, actual );
            }

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task awaits_inner_builder_when_validation_passes( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var expected = Mockery.Of<IDbConnectionFactory>();
                mockValidator.Setup( _ => _.ValidateAndThrow( connectionInfo, cancellationToken ) ).Returns( Task.CompletedTask );
                mockInner.Setup( _ => _.Build( connectionInfo, cancellationToken ) ).ReturnsAsync( expected );

                var actual = await method();
                Assert.Same( expected, actual );

                mockValidator.Verify( _ => _.ValidateAndThrow( connectionInfo, cancellationToken ), Times.Once() );
            }
        }
    }
}
