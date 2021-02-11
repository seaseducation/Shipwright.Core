﻿// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;

namespace Shipwright.Databases
{
    /// <summary>
    /// Fake database connection information.
    /// </summary>

    public record FakeConnectionInfo : DbConnectionInfo
    {
        public Guid Id { get; init; } = Guid.NewGuid();
    }
}