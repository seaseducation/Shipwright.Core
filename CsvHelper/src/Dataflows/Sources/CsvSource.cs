// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Shipwright.Dataflows.Sources
{
    /// <summary>
    /// Data source that reads values from a character-separated-value (CSV) file.
    /// </summary>

    public partial record CsvSource : Source
    {
        /// <summary>
        /// Full path to the source data file.
        /// </summary>

        public string Path { get; init; } = string.Empty;

        /// <summary>
        /// CsvHelper settings.
        /// Defaults to RFC-4180 using the current culture and UTF-8 encoding.
        /// </summary>

        public CsvConfiguration Settings { get; init; } = CsvConfigurations.CommaSeparatedRfc4180WithHeader( CultureInfo.CurrentCulture );

        /// <summary>
        /// Whether to throw an exception when the specified file is not found.
        /// A fatal log event will be added to the dataflow regardless of this value.
        /// </summary>

        public bool ThrowOnFileNotFound { get; init; } = true;

        /// <summary>
        /// Whether to throw an exception when reading is interrupted by encountering bad data.
        /// A fatal log event will be added to the dataflow regardless of this value.
        /// </summary>

        public bool ThrowOnBadData { get; init; } = true;

        /// <summary>
        /// Number of lines in the file to skip before starting CSV parsing.
        /// Due to its nature, normal CSV parsing rules will not apply to the skipped lines.
        /// This is intended for cases where header rows in files need to be ignored and not
        /// even have their number of fields counted by CSV helper.
        /// </summary>

        public int SkipLines { get; init; } = 0;
    }
}
