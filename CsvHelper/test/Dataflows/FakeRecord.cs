// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using Shipwright.Dataflows.Sources;
using Shipwright.Dataflows.Transformations;

namespace Shipwright.Dataflows
{
    public static class FakeRecord
    {
        /// <summary>
        /// Returns a fixture for generating records.
        /// </summary>

        public static Fixture Fixture()
        {
            var fixture = new Fixture();
            fixture.Register<Source>( () => new FakeSource() );
            fixture.Register<Transformation>( () => new FakeTransformation() );

            return fixture;
        }

        /// <summary>
        /// Returns a record.
        /// </summary>

        public static Record Create() => Fixture().Create<Record>();
    }
}
