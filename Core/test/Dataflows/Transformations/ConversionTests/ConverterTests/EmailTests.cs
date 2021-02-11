// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Shipwright.Dataflows.Transformations.Conversion;

namespace Shipwright.Dataflows.Transformations.ConversionTests.ConverterTests
{
    public class EmailTests
    {
        private object value;
        private object result;
        private bool method() => Converters.Email.Invoke( value, out result );

        [Theory]
        [InlineData( long.MaxValue )]
        [InlineData( int.MaxValue )]
        public void returns_false_when_not_text( object value )
        {
            this.value = value;
            Assert.False( method() );
            Assert.Null( result );
        }

        public static readonly IEnumerable<object[]> InvalidEmails = new[]
            {
                // built-in validator attribute catches these cases
                "Abc.example.com",
                "A@b@c@example.com",

                // built-in validator attribute does not catch these
                // these are caught by System.Net.Mail.MailAddress throwing an exception
                "a\"b(c)d,e:f; g<h> i[j\\k]l @example.com",
                "just\"not\"right@example.com",
                "this is\"not\\allowed@example.com",
                "this\\ still\\\"not\\\\allowed@example.com",

                // neither the built-in validator attribute nor System.Net.Mail.MailAddress catch cases
                // where the user is longer than 64 characters or the host is longer than 255 characters
                "1234567890123456789012345678901234567890123456789012345678901234+x@example.com",
                "x@1234567890123456789012345678901234567890123456789012345678901234123456789012345678901234567890123456789012345678901234567890123412345678901234567890123456789012345678901234567890123456789012341234567890123456789012345678901234567890123456789012345678901234example.com",

            }.Select( value => new object[] { value } ).ToArray();

        [Theory]
        [MemberData( nameof( InvalidEmails ) )]
        public void returns_false_when_email_not_valid( string value )
        {
            this.value = value;
            Assert.False( method() );
            Assert.Null( result );
        }

        public static readonly IEnumerable<object[]> ValidEmails = new[]
            {
                "simple@example.com",
                "very.common@example.com",
                "disposable.style.email.with+symbol@example.com",
                "other.email-with-hyphen@example.com",
                "fully-qualified-domain@example.com",
                "user.name+tag+sorting@example.com",
                "x@example.com",
                "example-indeed@strange-example.com",
                "admin@mailserver1",
                "example@s.example",
                "\" \"@example.org",
                "\"john..doe\"@example.org",
                "mailhost!username@example.org",
                "user%example.com@example.org",
            }.Select( value => new object[] { value } ).ToArray();

        [Theory, MemberData( nameof( ValidEmails ) )]
        public void returns_true_when_email_valid( string value )
        {
            this.value = value;
            Assert.True( method() );
            Assert.Equal( value, result );
        }
    }
}
