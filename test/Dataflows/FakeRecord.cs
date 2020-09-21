// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using Shipwright.Dataflows.Sources;
using System;
using System.Collections.Generic;

namespace Shipwright.Dataflows
{
    public record FakeRecord : Record
    {
        private static readonly Fixture fixture = new Fixture();
        public FakeRecord() : base( new FakeSource(), fixture.Create<IDictionary<string, object>>(), fixture.Create<int>(), StringComparer.OrdinalIgnoreCase ) { }
    }
}
