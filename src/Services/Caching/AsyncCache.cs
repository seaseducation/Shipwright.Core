// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Shipwright.Services.Caching
{
    /// <summary>
    /// Thread-safe cache for values that must be obtained asynchronously.
    /// </summary>
    /// <typeparam name="TKey">Type of the cache key.</typeparam>
    /// <typeparam name="TValue">Type of the cached value.</typeparam>

    public class AsyncCache<TKey, TValue> where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, AsyncLazy<TValue>> cache = new ConcurrentDictionary<TKey, AsyncLazy<TValue>>();

        /// <summary>
        /// Adds a key/value pair to the cache by using the given asynchronous function if the key
        /// does not already exist.
        /// </summary>
        /// <param name="key">Key of the item to return.</param>
        /// <param name="factory">Factory method to use for obtaining a new value.</param>
        /// <returns>The value for the key. This will either be the existing value (if one exists) or
        /// a new value created by the factory method.
        /// </returns>

        public virtual async Task<TValue> GetOrAdd( TKey key, Func<TKey, Task<TValue>> factory ) =>
            await cache.GetOrAdd( key, key => new AsyncLazy<TValue>( () => factory.Invoke( key ) ) );
    }
}
