// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Databases.OracleConnectionInfo;

namespace Shipwright.Databases.OracleConnectionInfoTests
{
    public class BuilderTests
    {
        private IDbConnectionBuilder<OracleConnectionInfo> instance() => new Builder();

        public class Build : BuilderTests
        {
            private OracleConnectionInfo connectionInfo;

            private Task<IDbConnectionFactory> method() => instance().Build( connectionInfo, default );

            [Fact]
            public async Task requires_connectionInfo()
            {
                connectionInfo = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( connectionInfo ), method );
            }

            [Fact]
            public async Task when_user_null_returns_factory_with_original_connection_string()
            {
                var fixture = new Fixture();
                var builder = new OracleConnectionStringBuilder
                {
                    DataSource = fixture.Create<string>(),
                    UserID = fixture.Create<string>(),
                    Password = fixture.Create<string>(),
                };

                connectionInfo = new OracleConnectionInfo { ConnectionString = builder.ToString(), UserId = null };

                var expected = connectionInfo.ConnectionString;
                var actual = await method();
                var typed = Assert.IsType<Factory>( actual );
                Assert.Equal( expected, typed.connectionString );
            }

            [Fact]
            public async Task when_user_given_returns_factory_with_modified_connection_string()
            {
                var fixture = new Fixture();
                var builder = new OracleConnectionStringBuilder
                {
                    DataSource = fixture.Create<string>(),
                    UserID = fixture.Create<string>(),
                    Password = fixture.Create<string>(),
                };

                connectionInfo = new OracleConnectionInfo { ConnectionString = builder.ToString(), UserId = fixture.Create<string>() };

                var expected = new OracleConnectionStringBuilder( connectionInfo.ConnectionString ) { UserID = connectionInfo.UserId }.ToString();
                var actual = await method();
                var typed = Assert.IsType<Factory>( actual );
                Assert.Equal( expected, typed.connectionString );
            }

            [Fact]
            public async Task when_password_null_returns_factory_with_original_connection_string()
            {
                var fixture = new Fixture();
                var builder = new OracleConnectionStringBuilder
                {
                    DataSource = fixture.Create<string>(),
                    UserID = fixture.Create<string>(),
                    Password = fixture.Create<string>(),
                };

                connectionInfo = new OracleConnectionInfo { ConnectionString = builder.ToString(), Password = null };

                var expected = connectionInfo.ConnectionString;
                var actual = await method();
                var typed = Assert.IsType<Factory>( actual );
                Assert.Equal( expected, typed.connectionString );
            }

            [Fact]
            public async Task when_password_given_returns_factory_with_modified_connection_string()
            {
                var fixture = new Fixture();
                var builder = new OracleConnectionStringBuilder
                {
                    DataSource = fixture.Create<string>(),
                    UserID = fixture.Create<string>(),
                    Password = fixture.Create<string>(),
                };

                connectionInfo = new OracleConnectionInfo { ConnectionString = builder.ToString(), Password = fixture.Create<string>() };

                var expected = new OracleConnectionStringBuilder( connectionInfo.ConnectionString ) { Password = connectionInfo.Password }.ToString();
                var actual = await method();
                var typed = Assert.IsType<Factory>( actual );
                Assert.Equal( expected, typed.connectionString );
            }
        }
    }
}
