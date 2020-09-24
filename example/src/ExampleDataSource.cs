// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using Shipwright.Dataflows;
using Shipwright.Dataflows.Sources;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Shipwright.Core.Example
{
    public record ExampleDataSource : Source
    {
        public int Records { get; init; }

        public class Validator : AbstractValidator<ExampleDataSource>
        {
            public Validator()
            {
                RuleFor( _ => _.Records ).GreaterThan( 0 );
            }
        }

        public class Handler : ISourceHandler<ExampleDataSource>
        {
            public async IAsyncEnumerable<Record> Read( ExampleDataSource source, Dataflow dataflow, [EnumeratorCancellation] CancellationToken cancellationToken )
            {
                if ( source == null ) throw new ArgumentNullException( nameof( source ) );
                if ( dataflow == null ) throw new ArgumentNullException( nameof( dataflow ) );

                for ( var i = 0; i < source.Records; i++ )
                {
                    var data = new Dictionary<string, object>
                    {
                        { "value", i },
                    };

                    yield return new Record( dataflow, source, data, i );
                }
            }
        }
    }
}
