// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using Xunit;
using static Shipwright.Dataflows.Transformations.Conversion;

namespace Shipwright.Dataflows.Transformations.ConversionTests.ConverterTests
{
    public class DecimalTests
    {
        private ConverterDelegate instance() => Converters.Decimal();

        private object value;
        private object result;
        private bool method() => instance().Invoke( value, out result );

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

        [Theory, ClassData( typeof( InconvertibleCases ) )]
        public void returns_false_when_not_convertible( object value )
        {
            this.value = value;

            var actual = method();
            Assert.False( actual );
            Assert.Null( result );
        }

        public class ConvertibleCases : TheoryData<object, object>
        {
            public ConvertibleCases()
            {
                // exact types
                Add( 1.0M, 1.0M );
                Add( decimal.MaxValue, decimal.MaxValue );

                // parsed text
                Add( "1", 1.0M );
                Add( "1.25", 1.25M );
                Add( "-42", -42M );

                // convertible
                Add( 1, 1.0M );
                Add( 3.1415, 3.1415M );
                Add( 0, 0M );
            }
        }

        [Theory, ClassData( typeof( ConvertibleCases ) )]
        public void returns_true_when_value_convertible( object value, object expected )
        {
            this.value = value;
            var actual = method();
            Assert.True( actual );
            Assert.Equal( expected, result );
        }
    }
}
