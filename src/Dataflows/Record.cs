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
        /// Dataflow in which the record is processed.
        /// </summary>

        public Dataflow Dataflow { get; init; }

        /// <summary>
        /// Source from which the record was read.
        /// </summary>

        public Source Source { get; init; }

        /// <summary>
        /// Constructs an instance to represent the given data within a dataflow.
        /// </summary>
        /// <param name="dataflow">Dataflow in which the record is processed.</param>
        /// <param name="source">Source from which the record was read.</param>
        /// <param name="data">Current values of the data in the record.</param>
        /// <param name="position">Ordinal position of the record within the dataflow.</param>

        public Record( Dataflow dataflow, Source source, IDictionary<string, object> data, long position )
        {
            Dataflow = dataflow ?? throw new ArgumentNullException( nameof( dataflow ) );
            Source = source ?? throw new ArgumentNullException( nameof( source ) );
            Data = data == null ? throw new ArgumentNullException( nameof( data ) ) : new Dictionary<string, object>( data, dataflow.FieldNameComparer );
            Origin = data.ToImmutableDictionary( dataflow.FieldNameComparer );
            Position = position;
        }
    }
}
