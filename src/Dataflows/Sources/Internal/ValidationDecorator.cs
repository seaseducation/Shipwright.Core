// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Shipwright.Validation;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Shipwright.Dataflows.Sources.Internal
{
    /// <summary>
    /// Decorates a source handler to add pre-execution validation support.
    /// </summary>
    /// <typeparam name="TSource">Type of the dataflow record source.</typeparam>

    public class ValidationDecorator<TSource> : SourceHandler<TSource> where TSource : Source
    {
        internal readonly ISourceHandler<TSource> inner;
        internal readonly IValidationAdapter<TSource> validator;

        /// <summary>
        /// Decorates a source handler to add pre-execution validation support.
        /// </summary>
        /// <param name="inner">Source handler to decorate.</param>
        /// <param name="validator">Validation adapter for the source type.</param>

        public ValidationDecorator( ISourceHandler<TSource> inner, IValidationAdapter<TSource> validator )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
            this.validator = validator ?? throw new ArgumentNullException( nameof( validator ) );
        }

        /// <summary>
        /// Throws an exception if validation of the source fails.
        /// </summary>
        /// <param name="source">Dataflow record source.</param>
        /// <param name="dataflow">Dataflow in which the source is read.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An awaitable stream of records.</returns>

        protected override async IAsyncEnumerable<Record> OnRead( TSource source, Dataflow dataflow, [EnumeratorCancellation] CancellationToken cancellationToken )
        {
            await validator.ValidateAndThrow( source, cancellationToken );

            await foreach ( var record in inner.Read( source, dataflow, cancellationToken ) )
            {
                yield return record;
            }
        }
    }
}
