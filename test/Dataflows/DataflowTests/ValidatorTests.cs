// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using AutoFixture.Xunit2;
using FluentValidation;
using Shipwright.Dataflows.Sources;
using Shipwright.Dataflows.Transformations;
using System.Collections.Generic;
using Xunit;
using static Shipwright.Dataflows.Dataflow;

namespace Shipwright.Dataflows.DataflowTests
{
    public class ValidatorTests
    {
        private readonly Fixture fixture = new Fixture();
        private readonly IValidator<Dataflow> validator = new Validator();

        public class Name : ValidatorTests
        {
            [Theory, ClassData( typeof( Cases.NullAndWhiteSpaceCases ) )]
            public void invalid_when_null_or_whitespace( string value ) => validator.InvalidWhen( _ => _.Name, value );

            [Theory, AutoData]
            public void valid_when_given( string value ) => validator.ValidWhen( _ => _.Name, value );
        }

        public class Sources : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Sources, null );

            [Fact]
            public void invalid_when_empty() => validator.InvalidWhen( _ => _.Sources, new List<Source>() );

            [Fact]
            public void invalid_when_null_element() => validator.InvalidWhen( _ => _.Sources, new List<Source>( fixture.CreateMany<FakeSource>() ) { null } );

            [Fact]
            public void valid_when_given_non_null_elements() => validator.ValidWhen( _ => _.Sources, new List<Source>( fixture.CreateMany<FakeSource>() ) );
        }

        public class Transformations : ValidatorTests
        {
            [Fact]
            public void invalid_when_null() => validator.InvalidWhen( _ => _.Transformations, null );

            [Fact]
            public void invalid_when_empty() => validator.InvalidWhen( _ => _.Transformations, new List<Transformation>() );

            [Fact]
            public void invalid_when_null_element() => validator.InvalidWhen( _ => _.Transformations, new List<Transformation>( fixture.CreateMany<FakeTransformation>() ) { null } );

            [Fact]
            public void valid_when_given_non_null_elements() => validator.ValidWhen( _ => _.Transformations, new List<Transformation>( fixture.CreateMany<FakeTransformation>() ) );
        }

        public class BufferSize : ValidatorTests
        {
            [Theory]
            [InlineData( -2 )]
            [InlineData( 0 )]
            public void invalid_when_not_positive_except_unbounded( int value ) => validator.InvalidWhen( _ => _.BufferSize, value );

            [Theory]
            [InlineData( -1 )]
            [AutoData]
            public void valid_when_unbounded_or_positive( int value ) => validator.ValidWhen( _ => _.BufferSize, value );
        }

        public class MaxDegreeOfParallelism : ValidatorTests
        {
            [Theory]
            [InlineData( -2 )]
            [InlineData( 0 )]
            public void invalid_when_not_positive_except_unbounded( int value ) => validator.InvalidWhen( _ => _.MaxDegreeOfParallelism, value );

            [Theory]
            [InlineData( -1 )]
            [AutoData]
            public void valid_when_unbounded_or_positive( int value ) => validator.ValidWhen( _ => _.MaxDegreeOfParallelism, value );
        }
    }
}
