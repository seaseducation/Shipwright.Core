// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using FluentValidation;
using System.Collections.Generic;
using Xunit;
using static Shipwright.Dataflows.Transformations.AggregateTransformation;

namespace Shipwright.Dataflows.Transformations.AggregateTransformationTests
{
    public class ValidatorTests
    {
        private readonly Fixture fixture = new Fixture();
        private readonly IValidator<AggregateTransformation> validator = new Validator();

        public class Sources : ValidatorTests
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
    }
}
