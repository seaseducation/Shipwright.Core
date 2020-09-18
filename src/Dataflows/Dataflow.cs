// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Commands;
using Shipwright.Dataflows.Sources;
using Shipwright.Dataflows.Transformations;
using System.Collections.Generic;

namespace Shipwright.Dataflows
{
    /// <summary>
    /// Command that executes an Extract-Transform-Load (ETL) dataflow.
    /// </summary>

    public partial record Dataflow : Command
    {
        /// <summary>
        /// Name of the dataflow.
        /// This value is ideally unique to an implementation and deterministic between executions.
        /// </summary>

        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Collection of dataflow record sources from which to read.
        /// </summary>

        public ICollection<Source> Sources { get; init; } = new List<Source>();

        /// <summary>
        /// Collection of transformations to execute against records in the dataflow.
        /// </summary>

        public ICollection<Transformation> Transformations { get; init; } = new List<Transformation>();
    }
}
