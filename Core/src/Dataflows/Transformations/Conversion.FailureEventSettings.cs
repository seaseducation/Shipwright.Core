// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Microsoft.Extensions.Logging;

namespace Shipwright.Dataflows.Transformations
{
    public partial record Conversion
    {
        /// <summary>
        /// Describes settings to use for conversion failure events.
        /// </summary>

        public record FailureEventSetting
        {
            /// <summary>
            /// Whether the conversion failure should stop record processing.
            /// Defaults to true.
            /// </summary>

            public bool IsFatal { get; set; } = true;

            /// <summary>
            /// Log level to use for lookup failure events.
            /// Defaults to <see cref="LogLevel.Error"/>.
            /// </summary>

            public LogLevel Level { get; set; } = LogLevel.Error;

            /// <summary>
            /// Delegate for generating failure event messages.
            /// </summary>

            public FailureEventMessageDelegate FailureEventMessage { get; init; } = field =>
                string.Format( Resources.CoreErrorMessages.ValueConversionFailed, field );

            /// <summary>
            /// Whether to remove the field from the dataflow if conversion fails.
            /// Defaults to true.
            /// </summary>

            public bool ClearField = true;
        }
    }
}
