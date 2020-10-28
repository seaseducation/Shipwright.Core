// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar;
using Lamar.Scanning.Conventions;
using Shipwright.Dataflows;
using System;

namespace Shipwright
{
    /// <summary>
    /// Extension methods for registering dataflow types with Lamar.
    /// </summary>

    public static class LamarDataflowExtensions
    {
        /// <summary>
        /// Registers all stock dataflow components.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry. </returns>

        public static ServiceRegistry AddDataflow( this ServiceRegistry registry ) => (registry ?? throw new ArgumentNullException( nameof( registry ) ))
            .AddSourceDispatch()
            .AddSourceValidation()
            .AddSourceCancellation()
            .AddTransformationDispatch()
            .AddTransformationValidation()
            .AddTransformationCancellation()
            .AddTransformationThrottling()
            .AddTransformationEventInspection();

        /// <summary>
        /// Adds all scanned implementations of dataflow components.
        /// </summary>
        /// <param name="scanner">Lamar assembly scanner.</param>
        /// <returns>The assembly scanner.</returns>

        public static IAssemblyScanner AddDataflowImplementations( this IAssemblyScanner scanner ) => (scanner ?? throw new ArgumentNullException( nameof( scanner ) ))
            .AddSourceHandlers()
            .AddTransformationFactories()
            .AddDataflowNotifications();

        /// <summary>
        /// Registers all Shipwright components.
        /// </summary>
        /// <param name="registry">Lamar service registry.</param>
        /// <returns>The service registry.</returns>

        public static ServiceRegistry AddAllShipwright( this ServiceRegistry registry ) => (registry ?? throw new ArgumentNullException( nameof( registry ) ))
            .AddValidationAdapter()
            .AddCommandDispatch()
            .AddCommandValidation()
            .AddCommandCancellation()
            .AddDbConnectionDispatch()
            .AddDbConnectionValidation()
            .AddDbConnectionCancellation()
            .AddDataflow();

        /// <summary>
        /// Adds all scanned Shipwright components.
        /// </summary>
        /// <param name="scanner">Lamar assembly scanner.</param>
        /// <returns>The assembly scanner.</returns>

        public static IAssemblyScanner AddAllShipwrightImplementations( this IAssemblyScanner scanner ) => (scanner ?? throw new ArgumentNullException( nameof( scanner ) ))
            .AddValidators()
            .AddCommandHandlers()
            .AddDbConnectionBuilders()
            .AddDataflowImplementations();
    }
}
