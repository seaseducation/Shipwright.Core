﻿// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Databases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwright.Dataflows.Transformations
{
    /// <summary>
    /// Transformation that inserts/updates database record based on current record values.
    /// </summary>

    public partial record DbUpsert : Transformation
    {
        /// <summary>
        /// Database connection information.
        /// </summary>

        public DbConnectionInfo ConnectionInfo { get; init; } = null!;

        /// <summary>
        /// Database table to which to write.
        /// </summary>

        public string Table { get; init; } = string.Empty;

        /// <summary>
        /// Helper for building DBMS-specific SQL statements.
        /// </summary>

        public Helper SqlHelper { get; init; } = null!;

        /// <summary>
        /// Collection of database field mappings.
        /// </summary>

        public ICollection<Mapping> Mappings { get; init; } = new List<Mapping>();

        /// <summary>
        /// Defines a delegate for executing code against a dataflow record.
        /// </summary>
        /// <param name="record">The dataflow record being transformed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>

        public delegate Task NotificationDelegate( Record record, CancellationToken cancellationToken );

        /// <summary>
        /// Delegate to execute only after a record is inserted.
        /// </summary>

        public NotificationDelegate OnInserted { get; init; } = ( record, ct ) => Task.CompletedTask;

        /// <summary>
        /// Delegate to execute only after a record is updated.
        /// </summary>

        public NotificationDelegate OnUpdated { get; init; } = ( record, ct ) => Task.CompletedTask;

        /// <summary>
        /// Delegate to execute only when the record is unchanged.
        /// </summary>

        public NotificationDelegate OnUnchanged { get; init; } = ( record, ct ) => Task.CompletedTask;

        /// <summary>
        /// Defines a delegate for generating an event message for when duplicate keys are detected.
        /// </summary>
        /// <param name="count">Number of matching records.</param>
        /// <returns>A formatted event message.</returns>

        public delegate string DuplicateKeyEventDelegate( int count );

        /// <summary>
        /// Delegate for generating an event message when the configured database key is not unique.
        /// </summary>

        public DuplicateKeyEventDelegate DuplicateKeyEventMessage { get; init; } = count =>
            string.Format( Resources.CoreErrorMessages.DbUpsertKeyNotUnique, count );
    }
}
