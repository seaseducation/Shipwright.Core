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
    /// Dispatcher for locating and building a factory for creating database connections
    /// for a <see cref="DbConnectionInfo"/> object.
    /// </summary>

    public class DbConnectionDispatcher : IDbConnectionDispatcher
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Constructs a dispatcher for locating and building a factory for creating database connections
        /// for a <see cref="DbConnectionInfo"/> object.
        /// </summary>
        /// <param name="serviceProvider">Dependency injection container or service provider.</param>

        public DbConnectionDispatcher( IServiceProvider serviceProvider )
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException( nameof( serviceProvider ) );
        }

        /// <summary>
        /// Locates the builder for the given <see cref="DbConnectionInfo"/> and builds the database
        /// connection factory.
        /// </summary>

        public async Task<IDbConnectionFactory> Build( DbConnectionInfo connectionInfo, CancellationToken cancellationToken )
        {
            if ( connectionInfo == null ) throw new ArgumentNullException( nameof( connectionInfo ) );

            var connectionType = connectionInfo.GetType();
            var builderType = typeof( IDbConnectionBuilder<> ).MakeGenericType( connectionType );

            dynamic builder = serviceProvider.GetService( builderType ) ??
                throw new InvalidOperationException( string.Format( Resources.CoreErrorMessages.MissingRequiredImplementation, builderType ) );

            // use of the dynamic type offloads the complex reflection, expression tree caching,
            // and delegate compilation to the DLR. this results in reflection overhead only applying
            // to the first call; subsequent calls perform similar to statically-compiled statements.
            return await builder.Build( (dynamic)connectionInfo, cancellationToken );
        }
    }
}
