// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Validation;

namespace Shipwright.Dataflows.Transformations
{
    /// <summary>
    /// Defines a dataflow record transformation.
    /// </summary>

    public abstract record Transformation : IRequiresValidation
    {
        /// <summary>
        /// Maximum number of concurrent records that can be transformed by the transformation's handler.
        /// Defaults to unlimited.
        /// </summary>

        public virtual int MaxDegreeOfParallelism { get; init; } = int.MaxValue;
    }
}
