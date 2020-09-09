// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using FluentValidation.Results;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Validation
{
    public class ValidationAdapterTests
    {
        public class ValidationOptional { }
        public class ValidationRequired : IRequiresValidation { }

        public class Context<T> where T : new()
        {
            public IList<IValidator<T>> validators = new List<IValidator<T>>();
            public IValidationAdapter<T> instance() => new ValidationAdapter<T>( validators );
        }

        public abstract class Constructor<T> where T : new()
        {
            private readonly Context<T> context = new Context<T>();

            [Fact]
            public void requires_validators()
            {
                context.validators = null!;
                Assert.Throws<ArgumentNullException>( nameof( context.validators ), context.instance );
            }
        }

        public class Constructor_ValidationOptional : Constructor<ValidationOptional> { }
        public class Constructor_ValidationRequired : Constructor<ValidationRequired> { }

        public abstract class Validate<T> where T : class, new()
        {
            protected Context<T> context = new Context<T>();
            private T instance = new T();
            protected CancellationToken cancellationToken;

            protected Task<ValidationResult> method() => context.instance().Validate( instance, cancellationToken );

            [Fact]
            public async Task requires_instance()
            {
                instance = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( instance ), method );
            }

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_success_when_all_validators_pass( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                var mockValidators = new List<Mock<IValidator<T>>>();

                for ( var i = 0; i < 3; i++ )
                {
                    var mockValidator = Mockery.Of( out IValidator<T> validator );
                    mockValidator.Setup( _ => _.ValidateAsync( instance, cancellationToken ) ).ReturnsAsync( new ValidationResult() );
                    mockValidators.Add( mockValidator );
                    context.validators.Add( validator );
                }

                var result = await method();
                Assert.True( result.IsValid );
                Assert.Empty( result.Errors );

                foreach ( var mockValidator in mockValidators )
                {
                    mockValidator.Verify( _ => _.ValidateAsync( instance, cancellationToken ), Times.Once() );
                }
            }

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_all_failures_when_validation_fails( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                var mockValidators = new List<Mock<IValidator<T>>>();
                var errors = new List<ValidationFailure>();

                for ( var i = 0; i < 3; i++ )
                {
                    var mockValidator = Mockery.Of( out IValidator<T> validator );
                    var error = new ValidationFailure( Guid.NewGuid().ToString(), Guid.NewGuid().ToString() );
                    mockValidator.Setup( _ => _.ValidateAsync( instance, cancellationToken ) ).ReturnsAsync( new ValidationResult( new[] { error } ) );
                    mockValidators.Add( mockValidator );
                    context.validators.Add( validator );
                    errors.Add( error );
                }

                var result = await method();
                Assert.False( result.IsValid );
                Assert.Equal( errors, result.Errors );

                foreach ( var mockValidator in mockValidators )
                {
                    mockValidator.Verify( _ => _.ValidateAsync( instance, cancellationToken ), Times.Once() );
                }
            }
        }

        public class Validate_ValidationOptional : Validate<ValidationOptional>
        {
            /// <summary>
            /// Validation for types that do not implement <see cref="IRequiresValidation"/> should succeed
            /// when no validators are defined.
            /// </summary>

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_success_when_no_validators( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                context.validators.Clear();

                var result = await method();
                Assert.True( result.IsValid );
                Assert.Empty( result.Errors );
            }
        }

        public class Validate_ValidationRequired : Validate<ValidationRequired>
        {
            /// <summary>
            /// Validation for types that implement <see cref="IRequiresValidation"/> should throw an exception
            /// when no validators are defined.
            /// </summary>

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task throws_when_no_validators( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                context.validators.Clear();
                var ex = await Assert.ThrowsAsync<InvalidOperationException>( method );
                Assert.Equal( string.Format( Resources.CoreErrorMessages.MissingRequiredValidator, typeof( ValidationRequired ) ), ex.Message );
            }
        }

        public abstract class ValidateAndThrow<T> where T : class, new()
        {
            protected Context<T> context = new Context<T>();
            private T instance = new T();
            protected CancellationToken cancellationToken;

            protected Task method() => context.instance().ValidateAndThrow( instance, cancellationToken );

            [Fact]
            public async Task requires_instance()
            {
                instance = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( instance ), method );
            }

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_when_all_validators_pass( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                var mockValidators = new List<Mock<IValidator<T>>>();

                for ( var i = 0; i < 3; i++ )
                {
                    var mockValidator = Mockery.Of( out IValidator<T> validator );
                    mockValidator.Setup( _ => _.ValidateAsync( instance, cancellationToken ) ).ReturnsAsync( new ValidationResult() );
                    mockValidators.Add( mockValidator );
                    context.validators.Add( validator );
                }

                await method();

                foreach ( var mockValidator in mockValidators )
                {
                    mockValidator.Verify( _ => _.ValidateAsync( instance, cancellationToken ), Times.Once() );
                }
            }

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task throws_when_validation_fails( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                var mockValidators = new List<Mock<IValidator<T>>>();
                var errors = new List<ValidationFailure>();

                for ( var i = 0; i < 3; i++ )
                {
                    var mockValidator = Mockery.Of( out IValidator<T> validator );
                    var error = new ValidationFailure( Guid.NewGuid().ToString(), Guid.NewGuid().ToString() );
                    mockValidator.Setup( _ => _.ValidateAsync( instance, cancellationToken ) ).ReturnsAsync( new ValidationResult( new[] { error } ) );
                    mockValidators.Add( mockValidator );
                    context.validators.Add( validator );
                    errors.Add( error );
                }

                var ex = await Assert.ThrowsAsync<ValidationException>( method );
                Assert.Equal( errors, ex.Errors );

                foreach ( var mockValidator in mockValidators )
                {
                    mockValidator.Verify( _ => _.ValidateAsync( instance, cancellationToken ), Times.Once() );
                }
            }
        }

        public class ValidateAndThrow_ValidationOptional : ValidateAndThrow<ValidationOptional>
        {
            /// <summary>
            /// Validation for types that do not implement <see cref="IRequiresValidation"/> should succeed
            /// when no validators are defined.
            /// </summary>

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task returns_success_when_no_validators( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                context.validators.Clear();

                await method();
            }
        }

        public class ValidateAndThrow_ValidationRequired : ValidateAndThrow<ValidationRequired>
        {
            /// <summary>
            /// Validation for types that implement <see cref="IRequiresValidation"/> should throw an exception
            /// when no validators are defined.
            /// </summary>

            [Theory]
            [ClassData( typeof( Cases.BooleanCases ) )]
            public async Task throws_when_no_validators( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                context.validators.Clear();
                var ex = await Assert.ThrowsAsync<InvalidOperationException>( method );
                Assert.Equal( string.Format( Resources.CoreErrorMessages.MissingRequiredValidator, typeof( ValidationRequired ) ), ex.Message );
            }
        }
    }
}
