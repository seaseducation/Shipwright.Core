// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows
{
    partial record Dataflow
    {
        /// <summary>
        /// Handler for executing dataflow commands.
        /// </summary>

        public class Handler : CommandHandler<Dataflow>
        {
            protected override Task Execute( Dataflow command, CancellationToken cancellationToken )
            {
                throw new NotImplementedException();
            }
        }
    }
}
