// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Shipwright.Dataflows.Transformations
{
    /// <summary>
    /// Transformation that writes record data to a character-separated-value (CSV) file.
    /// </summary>

    public partial record CsvOutput : Transformation
    {
        /// <summary>
        /// Full path to the output data file.
        /// </summary>

        public string Path { get; init; } = string.Empty;

        /// <summary>
        /// Whether to append to an existing file if one exists.
        /// When false, the file will be overwritten.
        /// Defaults to false.
        /// </summary>

        public bool Append { get; init; } = false;

        /// <summary>
        /// File encoding.
        /// Defaults to <see cref="Encoding.UTF8"/>.
        /// </summary>

        public Encoding Encoding { get; init; } = Encoding.UTF8;

        /// <summary>
        /// CsvHelper settings.
        /// Defaults to RFC-4180 using the current culture and UTF-8 encoding.
        /// </summary>

        public CsvConfiguration Settings { get; init; } = CsvConfigurations.CommaSeparatedRfc4180WithHeader( CultureInfo.CurrentCulture );

        /// <summary>
        /// Collection of fields mapped to the CSV output columns.
        /// When field name is null, blanks will be written to the output column.
        /// </summary>

        public ICollection<(string? field, string column)> Output { get; init; } = new List<(string?, string)>();

        /// <summary>
        /// Maximum number of concurrent records that can be transformed by the transformation's handler.
        /// Must be 1.
        /// </summary>

        public override int MaxDegreeOfParallelism { get; init; } = 1;
    }
}
