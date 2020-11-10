// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Moq;
using Shipwright.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Sources.DbSource;

namespace Shipwright.Dataflows.Sources.DbSourceTests
{
    public class HandlerTests
    {
        IDbConnectionDispatcher connectionDispatcher;
        ISourceHandler<DbSource> instance() => new Handler( connectionDispatcher );

        Mock<IDbConnectionDispatcher> mockConnectionDispatcher;

        public HandlerTests()
        {
            mockConnectionDispatcher = Mockery.Of( out connectionDispatcher );
        }

        public class Constructor : HandlerTests
        {
            [Fact]
            public void requires_connectionDispatcher()
            {
                connectionDispatcher = null!;
                Assert.Throws<ArgumentNullException>( nameof( connectionDispatcher ), instance );
            }
        }

        public class Read : HandlerTests
        {
            DbSource source = new DbSource();
            Dataflow dataflow = new Dataflow();
            async Task<List<Record>> method() => await instance().Read( source, dataflow, default ).ToListAsync();

            [Fact]
            public async Task requires_source()
            {
                source = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( source ), method );
            }

            [Fact]
            public async Task requires_dataflow()
            {
                dataflow = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( dataflow ), method );
            }
        }
    }
}
