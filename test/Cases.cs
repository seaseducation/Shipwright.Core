// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Shipwright
{
    /// <summary>
    /// Helpers for common test cases.
    /// </summary>

    public static class Cases
    {
        /// <summary>
        /// String value composed of all common whitespace characters.
        /// </summary>

        public static readonly string WhiteSpace = new string(
            new char[]
            {
                '\x0009', // horizontal tab
                '\x000a', // line feed
                '\x000b', // vertical tab
                '\x000c', // form feed
                '\x000d', // carriage return
                '\x0085', // next line
                '\x00a0', // non-breaking space
            } );

        /// <summary>
        /// XUnit test cases for empty and whitespace string values.
        /// </summary>

        public class WhiteSpaceCases : TheoryData<string>
        {
            public WhiteSpaceCases()
            {
                Add( string.Empty );
                Add( WhiteSpace );
            }
        }

        /// <summary>
        /// XUnit test cases for null, empty, and whitespace string values.
        /// </summary>

        public class NullAndWhiteSpaceCases : WhiteSpaceCases
        {
            public NullAndWhiteSpaceCases()
            {
                Add( null );
            }
        }

        /// <summary>
        /// Attribute for injecting null and whitespace cases.
        /// </summary>

        public class NullAndWhiteSpaceAttribute : DataAttribute
        {
            public override IEnumerable<object[]> GetData( MethodInfo testMethod )
            {
                var values = new[] { string.Empty, WhiteSpace, null };

                foreach ( var value in values )
                {
                    yield return new[] { value };
                }
            }
        }

        /// <summary>
        /// Collection of boolean cases.
        /// </summary>

        public class BooleanCases : TheoryData<bool>
        {
            public BooleanCases()
            {
                Add( true );
                Add( false );
            }
        }

        public class BooleanCasesAttribute : DataAttribute
        {
            public override IEnumerable<object[]> GetData( MethodInfo testMethod )
            {
                yield return new object[] { true };
                yield return new object[] { false };
            }
        }
    }
}
