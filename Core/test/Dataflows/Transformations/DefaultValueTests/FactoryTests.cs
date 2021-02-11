// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using System;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.DefaultValue;

namespace Shipwright.Dataflows.Transformations.DefaultValueTests
{
    public class FactoryTests
    {
        private ITransformationFactory<DefaultValue> instance() => new Factory();

        public class Create : FactoryTests
        {
            private DefaultValue transformation = new Fixture().Create<DefaultValue>();

            private Task<ITransformationHandler> method() => instance().Create( transformation, default );

            [Fact]
            public async Task requires_transformation()
            {
                transformation = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( transformation ), method );
            }

            [Fact]
            public async Task returns_handler()
            {
                var actual = await method();
                var typed = Assert.IsType<Handler>( actual );
                Assert.Same( transformation, typed.transformation );
            }
        }
    }
}
