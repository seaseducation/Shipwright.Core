// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;

namespace Shipwright.Dataflows.Transformations
{
    partial record CsvOutput
    {
        /// <summary>
        /// Defines a helper for wrapping CSV writing functionality.
        /// </summary>

        public interface IHelper : IAsyncDisposable
        {
            /// <summary>
            /// Writes the header record to the output file when working on a new file.
            /// </summary>

            void WriteHeader();

            /// <summary>
            /// Writes the given value to the next field.
            /// </summary>
            /// <param name="value">Value to write to the output file.</param>

            void WriteField( object? value );

            /// <summary>
            /// Advances the writer to the next record.
            /// </summary>

            void NextRecord();
        }
    }
}
