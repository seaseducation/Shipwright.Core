// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using FluentValidation.Validators;
using System.Collections.Generic;
using System.Linq;

namespace Shipwright.Dataflows.Transformations
{
    partial record CsvOutput
    {
        /// <summary>
        /// Validator for the <see cref="CsvOutput"/> transformation.
        /// </summary>

        public class Validator : AbstractValidator<CsvOutput>
        {
            /// <summary>
            /// Validator for the <see cref="CsvOutput"/> transformation.
            /// </summary>

            public Validator()
            {
                // checks to see that the column side of a mapping has a value
                bool HaveDefinedColumns( CsvOutput transformation, IEnumerable<(string?, string)> mappings, PropertyValidatorContext context )
                {
                    mappings ??= Enumerable.Empty<(string?, string)>();

                    foreach ( var (field, column) in mappings )
                    {
                        // null is allowed in field mappings
                        if ( column == null )
                        {
                            context.MessageFormatter.AppendArgument( "ValidationMessage", Resources.CoreErrorMessages.NoNullElementsValidationMessage );
                            return false;
                        }

                        if ( (field != null && string.IsNullOrWhiteSpace( field )) || string.IsNullOrWhiteSpace( column ) )
                        {
                            context.MessageFormatter.AppendArgument( "ValidationMessage", Resources.CoreErrorMessages.NoWhiteSpaceElementsValidationMessage );
                            return false;
                        }
                    }

                    return true;
                }

                RuleFor( _ => _.Path ).NotNull().NotWhiteSpace();
                RuleFor( _ => _.Encoding ).NotNull();
                RuleFor( _ => _.MaxDegreeOfParallelism ).Equal( 1 );
                RuleFor( _ => _.Output ).NotNull().Must( HaveDefinedColumns );
                RuleFor( _ => _.Settings ).NotNull();
            }
        }
    }
}
