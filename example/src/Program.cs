// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shipwright.Commands;
using Shipwright.Dataflows;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Core.Example
{
    public class Program : BackgroundService
    {
        private static async Task Main( string[] args )
        {
            await CreateHostBuilder( args )
                .RunConsoleAsync();
        }

        public static IHostBuilder CreateHostBuilder( string[] args ) =>
            Host.CreateDefaultBuilder( args )
                .UseLamar( ( context, registry ) =>
                {
                    registry
                        .AddValidationAdapter()
                        .AddCommandDispatch()
                        .AddCommandValidation()
                        .AddCommandCancellation()
                        .AddDataflow();

                    registry.Scan( scanner =>
                    {
                        scanner.AssemblyContainingType<Dataflow>();
                        scanner.AssemblyContainingType<Program>();

                        scanner
                            .AddValidators()
                            .AddCommandHandlers()
                            .AddDataflowImplementations();
                    } );

                    registry.AddHostedService<Program>();
                } );

        private readonly IHostApplicationLifetime lifetime;
        private readonly ICommandDispatcher dispatcher;

        public Program( IHostApplicationLifetime lifetime, ICommandDispatcher dispatcher )
        {
            this.lifetime = lifetime ?? throw new ArgumentNullException( nameof( lifetime ) );
            this.dispatcher = dispatcher ?? throw new ArgumentNullException( nameof( dispatcher ) );
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken )
        {
            var dataflow = new Dataflow
            {
                Name = "FizzBuzz",

                Sources =
                {
                    new ExampleDataSource { Records = 100 },
                },

                Transformations =
                {
                    new ExampleTransformation { Fizz = 3, Buzz = 5 },
                },
            };

            try
            {
                await dispatcher.Execute( dataflow, stoppingToken );
            }

            finally
            {
                lifetime.StopApplication();
            }
        }
    }
}
