// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Dapper;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Data;

namespace Shipwright.Databases
{
    public partial record OracleConnectionInfo
    {
        /// <summary>
        /// Factory for generating <see cref="OracleConnectionInfo"/> connections.
        /// </summary>

        public class Factory : IDbConnectionFactory
        {
            internal readonly string connectionString;

            /// <summary>
            /// Factory for generating <see cref="OracleConnectionInfo"/> connections.
            /// </summary>
            /// <param name="connectionString">Completed connection string.</param>

            public Factory( string connectionString )
            {
                this.connectionString =
                    new OracleConnectionStringBuilder( connectionString ) { ConnectionTimeout = 120 }.ToString();
            }

            /// <summary>
            /// Creates a database connection.
            /// </summary>

            public IDbConnection Create() => new OracleConnection( connectionString );

            /// <summary>
            /// Callback that closes clob/blob values to stop a memory leak.
            /// </summary>
            /// <param name="reader">Data reader at the current record position.</param>

            public void FinishReadingCurrentRecord( IDataReader reader )
            {
                if ( reader == null ) throw new ArgumentNullException( nameof( reader ) );

                // when reading from dapper, the data reader will be wrapped
                if ( reader is IWrappedDataReader wrapper )
                {
                    reader = wrapper.Reader;
                }

                // when any of the fields contain a blob/clob type, they are open memory streams
                // close them to avoid a memory leak
                if ( reader is OracleDataReader oracle )
                {
                    var count = reader.FieldCount;

                    for ( var i = 0; i < count; i++ )
                    {
                        var value = oracle.GetOracleValue( i );

                        switch ( value )
                        {
                            case OracleClob clob:
                                clob.Close();
                                break;

                            case OracleBlob blob:
                                blob.Close();
                                break;
                        }
                    }
                }

                // sanity check on the reader type
                else
                {
                    throw new NotImplementedException( $"Unexpected reader type: {reader.GetType()}" );
                }
            }
        }
    }
}
