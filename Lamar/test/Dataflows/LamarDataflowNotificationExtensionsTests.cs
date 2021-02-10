// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Lamar;
using Lamar.Scanning.Conventions;
using Shipwright.Dataflows.Notifications;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shipwright.Dataflows
{
    public class LamarDataflowNotificationExtensionsTests
    {
        private readonly ServiceRegistry registry = new ServiceRegistry();

        public class AddDataflowNotifications : LamarDataflowNotificationExtensionsTests
        {
            private IAssemblyScanner scanner;
            private IAssemblyScanner method() => scanner.AddDataflowNotifications();

            public class FakeNotificationReceiver : INotificationReceiver
            {
                public Task NotifyDataflowCompleted( Dataflow dataflow, CancellationToken cancellationToken ) => throw new NotImplementedException();

                public Task NotifyDataflowStarting( Dataflow dataflow, CancellationToken cancellationToken ) => throw new NotImplementedException();

                public Task NotifyRecordCompleted( Record record, CancellationToken cancellationToken ) => throw new NotImplementedException();
            }

            [Fact]
            public void requires_scanner()
            {
                scanner = null!;
                Assert.Throws<ArgumentNullException>( nameof( scanner ), method );
            }

            [Fact]
            public void registers_discovered_notifiers()
            {
                registry.Scan( scanner =>
                {
                    this.scanner = scanner;
                    scanner.AssemblyContainingType<Dataflow>(); // sanity check to ensure decorators aren't grabbed
                    scanner.AssemblyContainingType<AddDataflowNotifications>();

                    var actual = method();
                    Assert.Same( this.scanner, actual );
                } );

                using var container = new Container( registry );
                var instance = container.GetInstance<INotificationReceiver>();
                Assert.IsType<FakeNotificationReceiver>( instance );
            }
        }
    }
}
