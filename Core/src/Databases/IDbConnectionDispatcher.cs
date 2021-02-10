// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Databases
{
    /// <summary>
    /// Defines a dispatcher for locating and building a factory for creating database connections
    /// for a <see cref="DbConnectionInfo"/> object.
    /// </summary>

    public interface IDbConnectionDispatcher
    {
        /// <summary>
        /// Locates the builder for the given <see cref="DbConnectionInfo"/> and builds the database
        /// connection factory.
        /// </summary>
        /// <param name="connectionInfo">Database connection information.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A factory for creating database connections for the specified database.</returns>

        Task<IDbConnectionFactory> Build( DbConnectionInfo connectionInfo, CancellationToken cancellationToken );
    }
}
