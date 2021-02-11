// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture.Xunit2;
using System;
using System.Data;
using System.Data.SqlClient;
using Xunit;
using static Shipwright.Databases.SqlServerConnectionInfo;

namespace Shipwright.Databases.SqlServerConnectionInfoTests
{
    public class FactoryTests
    {
        private string connectionString = Guid.NewGuid().ToString();

        private IDbConnectionFactory instance() => new Factory( connectionString );

        public class Constructor : FactoryTests
        {
            [Fact]
            public void requires_connectionString()
            {
                connectionString = null!;
                Assert.Throws<ArgumentNullException>( nameof( connectionString ), instance );
            }
        }

        public class Create : FactoryTests
        {
            private IDbConnection method() => instance().Create();

            [Theory, AutoData]
            public void returns_npgsql_connection( string server, string database )
            {
                connectionString = new SqlConnectionStringBuilder { DataSource = server, InitialCatalog = database }.ToString();

                using var actual = method();
                var typed = Assert.IsType<SqlConnection>( actual );

                Assert.Equal( connectionString, typed.ConnectionString );
            }
        }
    }
}
