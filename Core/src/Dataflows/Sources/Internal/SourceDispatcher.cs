// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Shipwright.Dataflows.Sources.Internal
{
    /// <summary>
    /// Dispatcher for reading records from dataflow record sources.
    /// </summary>

    public class SourceDispatcher : ISourceDispatcher
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Constructs a dispatcher for reading records from dataflow record sources.
        /// </summary>
        /// <param name="serviceProvider">Dependency injection container or service provider.</param>
        /// <exception cref="ArgumentNullException">serviceProvider is null.</exception>

        public SourceDispatcher( IServiceProvider serviceProvider )
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException( nameof( serviceProvider ) );
        }

        /// <summary>
        /// Reads records from the given dataflow source.
        /// </summary>
        /// <param name="source">Dataflow record source.</param>
        /// <param name="dataflow">Dataflow in which the source is read.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An awaitable stream of dataflow records.</returns>

        public IAsyncEnumerable<Record> Read( Source source, Dataflow dataflow, CancellationToken cancellationToken )
        {
            if ( source == null ) throw new ArgumentNullException( nameof( source ) );

            var sourceType = source.GetType();
            var handlerType = typeof( ISourceHandler<> ).MakeGenericType( sourceType );

            dynamic handler = serviceProvider.GetService( handlerType ) ??
                throw new InvalidOperationException( string.Format( Resources.CoreErrorMessages.MissingRequiredImplementation, handlerType ) );

            // use of the dynamic type offloads the complex reflection, expression tree caching,
            // and delegate compilation to the DLR. this results in reflection overhead only applying
            // to the first call; subsequent calls perform similar to statically-compiled statements.
            return handler.Read( (dynamic)source, dataflow, cancellationToken );
        }
    }
}
