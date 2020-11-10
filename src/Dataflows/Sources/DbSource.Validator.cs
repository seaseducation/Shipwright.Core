// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using FluentValidation.Validators;
using System.Collections.Generic;
using System.Linq;

namespace Shipwright.Dataflows.Sources
{
    public partial record DbSource
    {
        /// <summary>
        /// Validator for the <see cref="DbSource"/> data source.
        /// </summary>

        public class Validator : AbstractValidator<DbSource>
        {
            /// <summary>
            /// Validator for the <see cref="DbSource"/> data source.
            /// </summary>

            public Validator()
            {
                // checks to see that both sides of each mapping has a value
                bool HaveDefinedValues( DbSource source, IEnumerable<(string, string)> mappings, PropertyValidatorContext context )
                {
                    mappings ??= Enumerable.Empty<(string, string)>();

                    foreach ( var (field, other) in mappings )
                    {
                        if ( field == null || other == null )
                        {
                            context.MessageFormatter.AppendArgument( "ValidationMessage", Resources.CoreErrorMessages.NoNullElementsValidationMessage );
                            return false;
                        }

                        if ( string.IsNullOrWhiteSpace( field ) || string.IsNullOrWhiteSpace( other ) )
                        {
                            context.MessageFormatter.AppendArgument( "ValidationMessage", Resources.CoreErrorMessages.NoWhiteSpaceElementsValidationMessage );
                            return false;
                        }
                    }

                    return true;
                }

                RuleFor( _ => _.ConnectionInfo ).NotNull();
                RuleFor( _ => _.Output ).NotNull().Must( HaveDefinedValues );
                RuleFor( _ => _.Sql ).NotNull().NotWhiteSpace();
            }
        }
    }
}
