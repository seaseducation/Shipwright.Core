// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Databases.Internal
{
    /// <summary>
    /// Decorates a connection builder to add pre-execution validation support.
    /// </summary>
    /// <typeparam name="TConnectionInfo">Type of the connection information.</typeparam>

    public class ValidationDecorator<TConnectionInfo> : IDbConnectionBuilder<TConnectionInfo> where TConnectionInfo : DbConnectionInfo
    {
        internal readonly IDbConnectionBuilder<TConnectionInfo> inner;
        internal readonly IValidationAdapter<TConnectionInfo> validator;

        /// <summary>
        /// Decorates a connection builder to add pre-execution validation support.
        /// </summary>
        /// <param name="inner">Builder to decorate.</param>
        /// <param name="validator">Validation adapter for the connection information type.</param>

        public ValidationDecorator( IDbConnectionBuilder<TConnectionInfo> inner, IValidationAdapter<TConnectionInfo> validator )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
            this.validator = validator ?? throw new ArgumentNullException( nameof( validator ) );
        }

        /// <summary>
        /// Throws an exception if validation of the connection information fails.
        /// Otherwise awaits execution of the decorated builder.
        /// </summary>
        /// <param name="connectionInfo">Connection whose factory to build.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A factory composed by the inner builder.</returns>

        public async Task<IDbConnectionFactory> Build( TConnectionInfo connectionInfo, CancellationToken cancellationToken )
        {
            if ( connectionInfo == null ) throw new ArgumentNullException( nameof( connectionInfo ) );

            await validator.ValidateAndThrow( connectionInfo, cancellationToken );
            return await inner.Build( connectionInfo, cancellationToken );
        }
    }
}
