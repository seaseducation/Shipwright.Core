// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Data;

namespace Shipwright.Databases
{
    /// <summary>
    /// Defines a factory for creating connections to the configured database.
    /// </summary>

    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Creates a connection to the configured database.
        /// </summary>
        /// <returns>A database connection.</returns>

        IDbConnection Create();

        /// <summary>
        /// Called when the data source has completed reading from the current record.
        /// This can be used to perform any post-processing of data values specific to the database provider.
        /// </summary>
        /// <param name="reader">Data reader at the current record position.</param>
        /// <remarks>
        /// This was added as a callback due to a memory leak when reading blob/clob data from Oracle.
        /// </remarks>

        void FinishReadingCurrentRecord( IDataReader reader ) { }
    }
}
