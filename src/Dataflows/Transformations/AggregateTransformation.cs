// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Collections.Generic;

namespace Shipwright.Dataflows.Transformations
{
    /// <summary>
    /// Transformation that consists of a collection of other transformations.
    /// </summary>

    public partial record AggregateTransformation : Transformation
    {
        /// <summary>
        /// Collection of record transformations to execute.
        /// </summary>

        public ICollection<Transformation> Transformations { get; init; } = new List<Transformation>();
    }
}
