// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Dataflows.Sources;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Shipwright.Dataflows
{
    /// <summary>
    /// Represents a data record within a dataflow.
    /// </summary>

    public record Record
    {
        /// <summary>
        /// Ordinal position of the record within the dataflow.
        /// </summary>

        public long Position { get; private set; }

        /// <summary>
        /// Current values of the data in the record.
        /// </summary>

        public IDictionary<string, object> Data { get; private set; }

        /// <summary>
        /// Starting values of the data as they were read from the source.
        /// </summary>

        public IReadOnlyDictionary<string, object> Origin { get; private set; }

        /// <summary>
        /// Source from which the record was read.
        /// </summary>

        public Source Source { get; init; }

        /// <summary>
        /// Constructs an instance to represent the given data within a dataflow.
        /// </summary>
        /// <param name="source">Source from which the record was read.</param>
        /// <param name="data">Current values of the data in the record.</param>
        /// <param name="position">Ordinal position of the record within the dataflow.</param>
        /// <param name="comparer">Comparer for record field name.</param>

        public Record( Source source, IDictionary<string, object> data, long position, StringComparer comparer )
        {
            Source = source ?? throw new ArgumentNullException( nameof( source ) );
            Data = data == null ? throw new ArgumentNullException( nameof( data ) ) : new Dictionary<string, object>( data, comparer );
            Origin = data.ToImmutableDictionary( comparer );
            Position = position;
        }
    }
}
