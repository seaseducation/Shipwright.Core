// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using Xunit;
using static Shipwright.Dataflows.Transformations.Conversion;

namespace Shipwright.Dataflows.Transformations.ConversionTests.ConverterTests
{
    public class DateTimeTests
    {
        private TryConvertDelegate instance() => Converters.DateTime;

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

                // out-of-range
                Add( long.MaxValue );
                Add( long.MinValue );

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
                // direct conversion
                Add( DateTime.MinValue, DateTime.MinValue );
                Add( DateTime.MaxValue, DateTime.MaxValue );

                // parsing
                Add( "2018-01-02 03:04:05", new DateTime( 2018, 01, 02, 03, 04, 05 ) );
                Add( "Jan  2, 2018 12:30 AM", new DateTime( 2018, 01, 02, 00, 30, 00 ) );
                Add( DateTime.MinValue.ToString( "o" ), DateTime.MinValue );
                Add( DateTime.MaxValue.ToString( "o" ), DateTime.MaxValue );
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
