// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture.Xunit2;
using Npgsql;
using System;
using System.Data;
using Xunit;
using static Shipwright.Databases.NpgsqlConnectionInfo;

namespace Shipwright.Databases.NpgsqlConnectionInfoTests
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
            public void returns_npgsql_connection( string host, string database )
            {
                connectionString = new NpgsqlConnectionStringBuilder() { Host = host, Database = database }.ToString();

                using var actual = method();
                var typed = Assert.IsType<NpgsqlConnection>( actual );

                Assert.Equal( connectionString, typed.ConnectionString );
            }
        }
    }
}
