// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations.Internal
{
    /// <summary>
    /// Decorates a transformation handler to add event inspection and behavior.
    /// </summary>

    public class EventInspectionHandlerDecorator : TransformationHandler
    {
        internal readonly ITransformationHandler inner;

        /// <summary>
        /// Decorates a transformation handler to add event inspection and behavior.
        /// </summary>

        public EventInspectionHandlerDecorator( ITransformationHandler inner )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
        }

        /// <summary>
        /// Inspects a record's events and skips processing when a fatal event is found.
        /// </summary>

        protected override async Task Transform( Record record, CancellationToken cancellationToken )
        {
            foreach ( var @event in record.Events )
            {
                if ( @event.IsFatal )
                {
                    return;
                }
            }

            await inner.Transform( record, cancellationToken );
        }
    }
}
