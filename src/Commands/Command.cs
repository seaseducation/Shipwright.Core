// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Validation;
using System;

namespace Shipwright.Commands
{
    /// <summary>
    /// Defines a command that returns a result when executed.
    /// </summary>
    /// <typeparam name="TResult">Type of the command result.</typeparam>

    public abstract record Command<TResult> : IRequiresValidation { }

    /// <summary>
    /// Defines a command that does not return a result when executed.
    /// </summary>

    public abstract record Command : Command<ValueTuple> { }
}
