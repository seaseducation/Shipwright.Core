// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Databases.Internal
{
    /// <summary>
    /// Decorates a connection builder to add pre-execution cancellation support.
    /// </summary>
    /// <typeparam name="TConnectionInfo">Type of the connection information.</typeparam>

    public class CancellationDecorator<TConnectionInfo> : IDbConnectionBuilder<TConnectionInfo> where TConnectionInfo : DbConnectionInfo
    {
        internal readonly IDbConnectionBuilder<TConnectionInfo> inner;

        /// <summary>
        /// Decorates a connection builder to add pre-execution cancellation support.
        /// </summary>
        /// <param name="inner">Connection builder to decorate.</param>

        public CancellationDecorator( IDbConnectionBuilder<TConnectionInfo> inner )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
        }

        /// <summary>
        /// Throws an exception if cancellation has been requested.
        /// Otherwise awaits execution of the decorated builder.
        /// </summary>
        /// <param name="connectionInfo">Connection whose factory to build.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The factory built by the inner builder.</returns>

        public async Task<IDbConnectionFactory> Build( TConnectionInfo connectionInfo, CancellationToken cancellationToken )
        {
            if ( connectionInfo == null ) throw new ArgumentNullException( nameof( connectionInfo ) );

            cancellationToken.ThrowIfCancellationRequested();
            return await inner.Build( connectionInfo, cancellationToken );
        }
    }
}
