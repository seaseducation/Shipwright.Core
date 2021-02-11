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
    public class HandlerTests
    {
        private Code transformation = new Code { };
        private ITransformationHandler instance() => new Handler( transformation );

        public class Constructor : HandlerTests
        {
            [Fact]
            public void requires_transformation()
            {
                transformation = null!;
                Assert.Throws<ArgumentNullException>( nameof( transformation ), instance );
            }
        }

        public class Transform : HandlerTests
        {
            private Record record = FakeRecord.Create();
            private CancellationToken cancellationToken;
            private Task method() => instance().Transform( record, cancellationToken );

            [Fact]
            public async Task requires_record()
            {
                record = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( record ), method );
            }

            [Theory, Cases.BooleanCases]
            public async Task executes_delegate( bool canceled )
            {
                cancellationToken = new CancellationToken( canceled );
                (Record record, CancellationToken cancellationToken) actual = default;

                transformation = transformation with { Delegate = async ( r, ct ) => actual = (r, ct) };

                await method();
                Assert.Same( record, actual.record );
                Assert.Equal( cancellationToken, actual.cancellationToken );
            }
        }
    }
}
