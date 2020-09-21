// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using FluentValidation;
using Moq;
using Shipwright.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Dataflows.Sources.Internal
{
    public class ValidationDecoratorTests
    {
        private ISourceHandler<FakeSource> inner;
        private IValidationAdapter<FakeSource> validator;

        private ISourceHandler<FakeSource> instance() => new ValidationDecorator<FakeSource>( inner, validator );

        private readonly Mock<ISourceHandler<FakeSource>> mockInner;
        private readonly Mock<IValidationAdapter<FakeSource>> mockValidator;

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

        public class Read : ValidationDecoratorTests
        {
            private FakeSource source = new FakeSource();
            private StringComparer comparer;
            private CancellationToken cancellationToken;

            private async Task<List<Record>> method() => await instance().Read( source, comparer, cancellationToken ).ToListAsync();

            [Fact]
            public async Task requires_source()
            {
                source = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( source ), method );
            }

            [Fact]
            public async Task rethrows_exception_from_validator()
            {
                var expected = new ValidationException( Guid.NewGuid().ToString() );
                mockValidator.Setup( _ => _.ValidateAndThrow( source, cancellationToken ) ).Throws( expected );

                var actual = await Assert.ThrowsAsync<ValidationException>( method );
                Assert.Same( expected, actual );
            }

            [Theory, ClassData( typeof( SourceArgumentCases ) )]
            public async Task returns_records( StringComparer comparer, bool canceled )
            {
                this.comparer = comparer;
                cancellationToken = new CancellationToken( canceled );

                var fixture = new Fixture();
                var expected = Enumerable.Range( 0, 3 ).Select( position => new Record( source, fixture.Create<IDictionary<string, object>>(), position, comparer ) ).ToArray();

                async IAsyncEnumerable<Record> callback()
                {
                    foreach ( var record in expected )
                    {
                        yield return record;
                    }
                }

                mockValidator.Setup( _ => _.ValidateAndThrow( source, cancellationToken ) ).Returns( Task.CompletedTask );
                mockInner.Setup( _ => _.Read( source, comparer, cancellationToken ) ).Returns( callback );

                var actual = await method();
                Assert.Equal( expected, actual );
            }
        }
    }
}
