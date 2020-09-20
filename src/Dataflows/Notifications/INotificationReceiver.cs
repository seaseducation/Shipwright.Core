// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Notifications
{
    /// <summary>
    /// Defines a receiver for dataflow notifications.
    /// </summary>

    public interface INotificationReceiver
    {
        /// <summary>
        /// Notifies the receiver that a dataflow is starting.
        /// </summary>
        /// <param name="dataflow">Dataflow that is starting.</param>
        /// <param name="cancellationToken">Cancellation token.</param>

        Task NotifyDataflowStarting( Dataflow dataflow, CancellationToken cancellationToken );

        /// <summary>
        /// Notifies the receiver that a dataflow has completed.
        /// </summary>
        /// <param name="dataflow">Dataflow that has completed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>

        Task NotifyDataflowCompleted( Dataflow dataflow, CancellationToken cancellationToken );

        /// <summary>
        /// Notifies the receiver that a dataflow record has completed processing.
        /// </summary>
        /// <param name="record">Dataflow record that has completed processing.</param>
        /// <param name="cancellationToken">Cancellation token.</param>

        Task NotifyRecordCompleted( Record record, CancellationToken cancellationToken );
    }
}
