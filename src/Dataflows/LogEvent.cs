// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Microsoft.Extensions.Logging;

namespace Shipwright.Dataflows
{
    /// <summary>
    /// Represents a loggable event encountered during a dataflow.
    /// </summary>

    public record LogEvent
    {
        /// <summary>
        /// Whether the event should stop record processing.
        /// A fatal event will stop all subsequent transformations from processing on the associated record.
        /// </summary>

        public bool IsFatal { get; init; }

        /// <summary>
        /// Severity of the event.
        /// </summary>

        public LogLevel Level { get; init; }

        /// <summary>
        /// Description of the event.
        /// </summary>

        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Value or values that provide context to the event.
        /// The value is ideally serializable.
        /// </summary>

        public object? Value { get; init; }
    }
}
