// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using Xunit;
using static Shipwright.Dataflows.Transformations.Conversion;

namespace Shipwright.Dataflows.Transformations.ConversionTests.ConverterTests
{
    public class IntegerTests
    {
        private ConverterDelegate instance() => Converters.Integer();

        private object value;
        private object result;
        private bool method() => instance().Invoke( value, out result );

        public class InconvertibleCases : TheoryData<object>
        {
            public InconvertibleCases()
            {
                Add( "187x" );
                Add( Guid.NewGuid().ToString() );
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
                Add( "0123", 123 );
                Add( (long)42, 42 );
                Add( (byte)42, 42 );
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

        [Fact]
        public void returns_true_when_value_is_integer()
        {
            value = new Random().Next();
            var actual = method();
            Assert.True( actual );
            Assert.Equal( value, result );
        }
    }
}
