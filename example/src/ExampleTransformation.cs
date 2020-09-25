// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using Shipwright.Dataflows;
using Shipwright.Dataflows.Transformations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Core.Example
{
    public record ExampleTransformation : Transformation
    {
        public int Fizz { get; init; }

        public int Buzz { get; init; }

        public class Validator : AbstractValidator<ExampleTransformation>
        {
            public Validator()
            {
                RuleFor( _ => _.Fizz ).GreaterThan( 0 );
                RuleFor( _ => _.Buzz ).GreaterThan( 0 );
                RuleFor( _ => _ ).Must( _ => _.Fizz < _.Buzz );
            }
        }

        public class Handler : ITransformationHandler
        {
            public int Fizz { get; init; }
            public int Buzz { get; init; }
            public int FizzBuzz { get; init; }

            public async ValueTask DisposeAsync() { }

            public async Task Transform( Record record, CancellationToken cancellationToken )
            {
                if ( record.Data.TryGetValue( "value", out var value ) )
                {
                    var output = value switch
                    {
                        int i when i % FizzBuzz == 0 => "fizzbuzz",
                        int i when i % Fizz == 0 => "fizz",
                        int i when i % Buzz == 0 => "buzz",
                        _ => value
                    };

                    Console.WriteLine( $"{value}: {output}" );
                }
            }
        }

        public class Factory : ITransformationFactory<ExampleTransformation>
        {
            public async Task<ITransformationHandler> Create( ExampleTransformation transformation, CancellationToken cancellationToken )
            {
                if ( transformation == null ) throw new ArgumentNullException( nameof( transformation ) );

                return new Handler
                {
                    Fizz = transformation.Fizz,
                    Buzz = transformation.Buzz,
                    FizzBuzz = transformation.Fizz * transformation.Buzz,
                };
            }
        }
    }
}
