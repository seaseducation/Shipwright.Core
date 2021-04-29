// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Shipwright.Dataflows.Transformations
{
    public partial record Conversion
    {
        /// <summary>
        /// Pre-defined conversion delegates.
        /// </summary>

        public static class Converters
        {
            /// <summary>
            /// Creates a converter for integer values.
            /// </summary>

            public static ConverterDelegate Integer( NumberStyles styles = NumberStyles.Any, CultureInfo? cultureInfo = null ) => delegate ( object value, out object? result )
            {
                cultureInfo ??= CultureInfo.CurrentCulture;

                if ( value is int converted || (value is string text && int.TryParse( text, styles, cultureInfo, out converted )) )
                {
                    result = converted;
                    return true;
                }

                if ( value is IConvertible convertible )
                {
                    try
                    {
                        result = Convert.ToInt32( convertible );
                        return true;
                    }

                    catch { }
                }

                result = null;
                return false;
            };

            /// <summary>
            /// Basic email validator.
            /// </summary>

            private static readonly EmailAddressAttribute emailValidator = new EmailAddressAttribute();

            /// <summary>
            /// Validator for email addresses.
            /// </summary>

            public static bool Email( object value, out object? result )
            {
                if ( value is string text && emailValidator.IsValid( text ) )
                {
                    try
                    {
                        var address = new System.Net.Mail.MailAddress( text );

                        if ( address.User.Length <= 64 && address.Host.Length <= 255 )
                        {
                            result = text;
                            return true;
                        }
                    }

                    catch { }
                }

                result = null;
                return false;
            }

            /// <summary>
            /// Basic boolean converter.
            /// </summary>

            public static bool Boolean( object value, out object? result )
            {
                if ( value is bool converted )
                {
                    result = converted;
                    return true;
                }

                else if ( value is string text )
                {
                    if ( bool.TryParse( text, out converted ) )
                    {
                        result = converted;
                        return true;
                    }
                }

                else if ( value is IConvertible convertible )
                {
                    try
                    {
                        result = Convert.ToBoolean( convertible );
                        return true;
                    }

                    catch { }
                }

                result = null;
                return false;
            }

            /// <summary>
            /// Date converter that yields a <see cref="DateTime"/> value with no time component.
            /// </summary>

            public static bool Date( object value, out object? result )
            {
                if ( DateTime( value, out result ) && result is DateTime converted )
                {
                    result = converted.Date;
                    return true;
                }

                result = null;
                return false;
            }

            /// <summary>
            /// Date and time converter that yields a <see cref="DateTime"/>.
            /// </summary>

            public static bool DateTime( object value, out object? result )
            {
                if ( value is DateTime converted )
                {
                    result = converted;
                    return true;
                }

                else if ( value is string text )
                {
                    if ( System.DateTime.TryParse( text, out converted ) )
                    {
                        result = converted;
                        return true;
                    }
                }

                else if ( value is IConvertible convertible )
                {
                    try
                    {
                        result = Convert.ToDateTime( convertible );
                        return true;
                    }

                    catch { }
                }

                result = null;
                return false;
            }

            /// <summary>
            /// Creates a converter for decimal values.
            /// </summary>

            public static ConverterDelegate Decimal( NumberStyles styles = NumberStyles.Any, CultureInfo? cultureInfo = null ) => delegate ( object value, out object? result )
            {
                cultureInfo ??= CultureInfo.CurrentCulture;

                if ( value is decimal converted || (value is string text && decimal.TryParse( text, styles, cultureInfo, out converted )) )
                {
                    result = converted;
                    return true;
                }

                if ( value is IConvertible convertible )
                {
                    try
                    {
                        result = Convert.ToDecimal( convertible );
                        return true;
                    }

                    catch { }
                }

                result = null;
                return false;
            };
        }
    }
}
