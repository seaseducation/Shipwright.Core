#pragma warning disable IDE0073
// SPDX-License-Identifier: MIT
// Source: https://gist.github.com/seanterry/e2937b9172f9e0aedfeac88d77eb2f2d

/**************************************************************************************************
Copyright 2020 Sean Terry (seanterry@outlook.com)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute,
sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**************************************************************************************************/

namespace Moq
{
    /// <summary>
    /// Helper methods for working with Moq.
    /// </summary>

    public static partial class Mockery
    {
        /// <summary>
        /// Returns a mocked instance of the specified type.
        /// </summary>
        /// <typeparam name="T">Type to mock.</typeparam>
        /// <param name="behavior">Mock behavior. Defaults to Strict.</param>

        public static T Of<T>( MockBehavior behavior = MockBehavior.Strict ) where T : class =>
            new Mock<T>( behavior ).Object;

        /// <summary>
        /// Returns a mock of the specified type.
        /// </summary>
        /// <typeparam name="T">Type to mock.</typeparam>
        /// <param name="behavior">Mock behavior.</param>
        /// <param name="mocked">Instance of the mocked type.</param>

        public static Mock<T> Of<T>( MockBehavior behavior, out T mocked ) where T : class
        {
            var mock = new Mock<T>( behavior );
            mocked = mock.Object;
            return mock;
        }

        /// <summary>
        /// Returns a strict mock of the specified type.
        /// </summary>
        /// <typeparam name="T">Type to mock.</typeparam>
        /// <param name="mocked">Instance of the mocked type.</param>

        public static Mock<T> Of<T>( out T mocked ) where T : class => Of( MockBehavior.Strict, out mocked );
    }
}
