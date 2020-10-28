// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

namespace Shipwright.Dataflows.Transformations
{
    /// <summary>
    /// Defines a transformation that limits the number of concurrent records that can be transformed.
    /// </summary>

    public abstract record ThrottleableTransformation : Transformation
    {
        /// <summary>
        /// Maximum number of concurrent records that can be transformed by the transformation's handler.
        /// </summary>

        public int MaxDegreeOfParallelism { get; init; } = int.MaxValue;
    }
}
