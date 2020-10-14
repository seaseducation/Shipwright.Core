// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Databases
{
    /// <summary>
    /// Defines a builder for creating database connection factories.
    /// </summary>
    /// <typeparam name="TConnectionInfo">Type of the database connection information.</typeparam>
    /// <remarks>
    /// This abstration exists to support those cases where complex logic may be needed to compose
    /// the root information for connecting to a database. For example, in a multi-tenant application
    /// where tenant databases may exist on different services, this allows an awaitable means of
    /// locating a specific database using an external service.
    /// </remarks>

    public interface IDbConnectionBuilder<TConnectionInfo> where TConnectionInfo : DbConnectionInfo
    {
        /// <summary>
        /// Builds a factory for creating database connections for the given connection information.
        /// </summary>
        /// <param name="connectionInfo">Connection information of the database whose connection factory to build.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A factory for creating database connections.</returns>

        Task<IDbConnectionFactory> Build( TConnectionInfo connectionInfo, CancellationToken cancellationToken );
    }
}
