// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using Xunit;
using static Shipwright.Dataflows.Transformations.Conversion;

namespace Shipwright.Dataflows.Transformations.ConversionTests.ConverterTests
{
    public class BooleanTests
    {
        private object value;
        private object result;
        private bool method() => Converters.Boolean.Invoke( value, out result );

        public class ConvertibleCases : TheoryData<object, bool>
        {
            public ConvertibleCases()
            {
                // exact types
                Add( true, true );
                Add( false, false );

                // parsed text
                Add( "true", true );
                Add( "True", true );
                Add( "TRUE", true );
                Add( "false", false );
                Add( "FALSE", false );
                Add( "False", false );

                // convertible
                Add( 1, true );
                Add( -1, true );
                Add( 0, false );
            }
        }

        [Theory, ClassData( typeof( ConvertibleCases ) )]
        public void returns_true_when_convertible( object value, bool expected )
        {
            this.value = value;
            var actual = method();
            Assert.True( actual );
            Assert.Equal( expected, result );
        }

        public class InconvertibleCases : TheoryData<object>
        {
            public InconvertibleCases()
            {
                // null/whitespace values
                Add( null );
                Add( Cases.WhiteSpace );

                // text that can't be parsed
                Add( Guid.NewGuid().ToString() );

                // inconvertible types
                Add( Guid.NewGuid() );
            }
        }

        [Theory]
        [ClassData( typeof( InconvertibleCases ) )]
        public void fails_when_not_convertible( object value )
        {
            this.value = value;
            var actual = method();
            Assert.False( actual );
            Assert.Null( result );
        }
    }
}
