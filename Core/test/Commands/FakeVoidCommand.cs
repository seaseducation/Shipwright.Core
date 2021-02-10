// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;

namespace Shipwright.Commands
{
    /// <summary>
    /// Fake command for testing.
    /// </summary>

    public record FakeVoidCommand : Command
    {
        public Guid Value { get; init; } = Guid.NewGuid();
    }
}
