// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using CsvHelper.Configuration;
using System.Globalization;

namespace CsvHelper
{
    /// <summary>
    /// Pre-defined CsvHelper configurations.
    /// </summary>

    public static class CsvConfigurations
    {
        /// <summary>
        /// Default handler for unescaped quotes.
        /// </summary>
        /// <param name="context">CSV reader context.</param>

        private static void OnUnescapedQuote( ReadingContext context ) =>
            throw new BadDataException( context, string.Format( Shipwright.Resources.CsvHelperMessages.UnescapedQuote, context.ReaderConfiguration.Quote ) );

        /// <summary>
        /// Configuration for RFC-4180 compliant comma-separated files.
        /// </summary>
        /// <param name="cultureInfo"><see cref="CultureInfo"/> used to read and write files.</param>
        /// <param name="newLine"><see cref="NewLine"/> to use when writing files. Defaults to <see cref="NewLine.Environment"/>.</param>
        /// <param name="trimOptions">Field trimming options. Defaults to <see cref="TrimOptions.Trim"/>.</param>
        /// <returns>A configuration instance for RFC-4180 comma-separated files.</returns>

        public static CsvConfiguration CommaSeparatedRfc4180WithHeader( CultureInfo cultureInfo, NewLine newLine = NewLine.Environment, TrimOptions trimOptions = TrimOptions.Trim ) => new CsvConfiguration( cultureInfo )
        {
            AllowComments = false,
            BadDataFound = OnUnescapedQuote,
            Delimiter = ",",
            DetectColumnCountChanges = true,
            Escape = '"',
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            IgnoreQuotes = false,
            LineBreakInQuotedFieldIsBadData = false,
            NewLine = newLine,
            Quote = '"',
            TrimOptions = trimOptions,
        };
    }
}
