// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Collections.Generic;

namespace Shipwright.Dataflows.Transformations
{
    /// <summary>
    /// Transformation that adds a default value to record fields whose values are missing, null,
    /// or (optionally) blank/whitespace.
    /// </summary>

    public partial record DefaultValue : Transformation
    {
        /// <summary>
        /// Collection of default values mapped to their field names.
        /// </summary>

        public ICollection<(string field, Func<object> factory)> Defaults { get; init; } = new List<(string, Func<object>)>();

        /// <summary>
        /// Whether to overwrite blank/whitespace text values with the default value.
        /// Defaults to false.
        /// </summary>

        public bool DefaultOnBlank { get; init; }
    }
}
