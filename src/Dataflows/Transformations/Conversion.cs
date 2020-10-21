// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Collections.Generic;

namespace Shipwright.Dataflows.Transformations
{
    /// <summary>
    /// Transformation that performs a data conversion.
    /// </summary>

    public partial record Conversion : Transformation
    {
        /// <summary>
        /// Fields that will be converted.
        /// </summary>

        public ICollection<string> Fields { get; init; } = new List<string>();

        /// <summary>
        /// Defines a delegate for attempting to convert a value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="result">Converted value, when successful.</param>
        /// <returns>True when conversion is successful; otherwise false.</returns>

        public delegate bool TryConvertDelegate( object value, out object? result );

        /// <summary>
        /// Delegate for converting values.
        /// </summary>

        public TryConvertDelegate Converter { get; init; } = null!;

        /// <summary>
        /// Defines a delegate for generating an event message for conversion failures.
        /// </summary>
        /// <param name="field">Name of the field.</param>
        /// <returns>A formatted event message.</returns>

        public delegate string FailureEventMessageDelegate( string field );

        /// <summary>
        /// Setting that defines the event logged when the conversion fails.
        /// </summary>

        public FailureEventSetting ConversionFailedEvent { get; init; } = new FailureEventSetting();
    }
}
