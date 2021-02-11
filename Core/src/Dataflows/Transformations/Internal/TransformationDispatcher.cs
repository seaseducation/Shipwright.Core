// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations.Internal
{
    /// <summary>
    /// Dispatcher for delegating creation of transformation handlers to the appropriate factory.
    /// </summary>

    public class TransformationDispatcher : ITransformationDispatcher
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Constructs a dispatcher for delegating creation of transformation handlers to the appropriate factory.
        /// </summary>
        /// <param name="serviceProvider">Dependency injection container or service provider.</param>
        /// <exception cref="ArgumentNullException">serviceProvider is null.</exception>

        public TransformationDispatcher( IServiceProvider serviceProvider )
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException( nameof( serviceProvider ) );
        }

        /// <summary>
        /// Locates and invokes the factory for creating a handler for the given transformation.
        /// </summary>
        /// <param name="transformation">Transformation whose handler to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The handler for the transformation.</returns>

        public async Task<ITransformationHandler> Create( Transformation transformation, CancellationToken cancellationToken )
        {
            if ( transformation == null ) throw new ArgumentNullException( nameof( transformation ) );

            var transformationType = transformation.GetType();
            var factoryType = typeof( ITransformationFactory<> ).MakeGenericType( transformationType );

            dynamic factory = serviceProvider.GetService( factoryType ) ??
                throw new InvalidOperationException( string.Format( Resources.CoreErrorMessages.MissingRequiredImplementation, factoryType ) );

            // use of the dynamic type offloads the complex reflection, expression tree caching,
            // and delegate compilation to the DLR. this results in reflection overhead only applying
            // to the first call; subsequent calls perform similar to statically-compiled statements.
            return await factory.Create( (dynamic)transformation, cancellationToken );
        }
    }
}
