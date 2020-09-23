// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar.Scanning.Conventions;
using Shipwright.Dataflows.Notifications;
using System;

namespace Shipwright.Dataflows
{
    /// <summary>
    /// Extension methods for registering dataflow notification types with Lamar.
    /// </summary>

    public static class LamarDataflowNotificationExtensions
    {
        /// <summary>
        /// Adds all implementations of <see cref="INotificationReceiver"/> found in the scanned assemblies.
        /// </summary>
        /// <param name="scanner">Lamar assembly scanner.</param>
        /// <returns>The assembly scanner.</returns>

        public static IAssemblyScanner AddDataflowNotifications( this IAssemblyScanner scanner )
        {
            if ( scanner == null ) throw new ArgumentNullException( nameof( scanner ) );

            scanner.AddAllTypesOf<INotificationReceiver>();
            return scanner;
        }
    }
}
