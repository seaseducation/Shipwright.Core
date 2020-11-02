// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.Code;

namespace Shipwright.Dataflows.Transformations.CodeTests
{
    public class FactoryTests
    {
        private ITransformationFactory<Code> instance() => new Factory();

        public class Create : FactoryTests
        {
            private Code transformation = new Code();
            private CancellationToken cancellationToken;

            private Task<ITransformationHandler> method() => instance().Create( transformation, cancellationToken );

            [Fact]
            public async Task requires_transformation()
            {
                transformation = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( transformation ), method );
            }

            [Theory, Cases.BooleanCases]
            public async Task returns_handler( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );

                var actual = await method();
                var typed = Assert.IsType<Handler>( actual );
                Assert.Same( transformation, typed.transformation );
            }
        }
    }
}
