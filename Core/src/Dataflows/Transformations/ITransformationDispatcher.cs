// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    /// <summary>
    /// Defines a dispatcher for delegating creation of transformation handlers to the appropriate factory.
    /// </summary>

    public interface ITransformationDispatcher
    {
        /// <summary>
        /// Locates and invokes the factory for creating a handler for the given transformation.
        /// </summary>
        /// <param name="transformation">Transformation whose handler to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The handler for the transformation.</returns>

        Task<ITransformationHandler> Create( Transformation transformation, CancellationToken cancellationToken );
    }
}
