// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Collections.Generic;

namespace Shipwright.Dataflows.Sources
{
    /// <summary>
    /// Record source that consists of a collection of other sources.
    /// </summary>

    public partial record AggregateSource : Source
    {
        /// <summary>
        /// Collection of data sources from which to read.
        /// </summary>

        public ICollection<Source> Sources { get; init; } = new List<Source>();
    }
}
