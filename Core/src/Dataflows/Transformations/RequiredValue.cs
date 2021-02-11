// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Shipwright.Dataflows.Transformations
{
    /// <summary>
    /// Transformation that ensures that required fields are present and have values.
    /// </summary>

    public partial record RequiredValue : Transformation
    {
        /// <summary>
        /// Names of the fields that should have values.
        /// </summary>

        public ICollection<string> Fields { get; init; } = new List<string>();

        /// <summary>
        /// Whether to consider blank/whitespace values as valid.
        /// Defaults to true.
        /// </summary>

        public bool AllowBlank { get; init; } = true;

        /// <summary>
        /// Whether violations should stop record processing.
        /// Defaults to true.
        /// </summary>

        public bool ViolationIsFatal { get; init; } = true;

        /// <summary>
        /// Log level to use for violation events.
        /// Defaults to <see cref="LogLevel.Error"/>.
        /// </summary>

        public LogLevel ViolationLogLevel { get; init; } = LogLevel.Error;

        /// <summary>
        /// Defines a delegate for building the description of violation events.
        /// </summary>
        /// <param name="field">Name of the required field.</param>
        /// <returns>A violation event description.</returns>

        public delegate string ViolationDescriptionDelegate( string field );

        /// <summary>
        /// Optional delegate for building violation event descriptions.
        /// </summary>

        public ViolationDescriptionDelegate ViolationDescription { get; init; } =
            field => string.Format( Resources.CoreErrorMessages.MissingRequiredFieldValue, field );
    }
}
