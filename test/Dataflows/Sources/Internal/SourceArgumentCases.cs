// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using Xunit;

namespace Shipwright.Dataflows.Sources.Internal
{
    public class SourceArgumentCases : TheoryData<StringComparer, bool>
    {
        public SourceArgumentCases()
        {
            var comparers = new[] { StringComparer.Ordinal, StringComparer.OrdinalIgnoreCase };
            var booealns = new[] { true, false };

            foreach ( var comparer in comparers )
            {
                foreach ( var canceled in booealns )
                {
                    Add( comparer, canceled );
                }
            }
        }
    }
}
