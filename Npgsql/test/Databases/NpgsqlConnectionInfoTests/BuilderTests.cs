// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using Npgsql;
using System;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Databases.NpgsqlConnectionInfo;

namespace Shipwright.Databases.NpgsqlConnectionInfoTests
{
    public class BuilderTests
    {
        private IDbConnectionBuilder<NpgsqlConnectionInfo> instance() => new Builder();

        public class Build : BuilderTests
        {
            private NpgsqlConnectionInfo connectionInfo;

            private Task<IDbConnectionFactory> method() => instance().Build( connectionInfo, default );

            [Fact]
            public async Task requires_connectionInfo()
            {
                connectionInfo = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( connectionInfo ), method );
            }

            [Fact]
            public async Task when_database_null_returns_factory_with_original_connection_string()
            {
                var fixture = new Fixture();
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = fixture.Create<string>(),
                    Database = fixture.Create<string>(),
                };

                connectionInfo = new NpgsqlConnectionInfo { ConnectionString = builder.ToString(), Database = null };

                var expected = connectionInfo.ConnectionString;
                var actual = await method();
                var typed = Assert.IsType<Factory>( actual );
                Assert.Equal( expected, typed.connectionString );
            }

            [Fact]
            public async Task when_database_given_returns_factory_with_modified_connection_string()
            {
                var fixture = new Fixture();
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = fixture.Create<string>(),
                    Database = fixture.Create<string>(),
                };

                connectionInfo = new NpgsqlConnectionInfo { ConnectionString = builder.ToString(), Database = fixture.Create<string>() };

                var expected = new NpgsqlConnectionStringBuilder( connectionInfo.ConnectionString ) { Database = connectionInfo.Database }.ToString();
                var actual = await method();
                var typed = Assert.IsType<Factory>( actual );
                Assert.Equal( expected, typed.connectionString );
            }
        }
    }
}
