// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using FluentValidation.Validators;
using System.Collections.Generic;

namespace Shipwright.Dataflows.Transformations
{
    public partial record DbUpsert
    {
        /// <summary>
        /// Validator for <see cref="DbUpsert"/> transformations.
        /// </summary>

        public class Validator : AbstractValidator<DbUpsert>
        {
            /// <summary>
            /// Validator for <see cref="DbUpsert"/> transformations.
            /// </summary>

            public Validator()
            {
                bool HaveDefinedFieldNames( DbUpsert transformation, IEnumerable<Mapping> mappings, PropertyValidatorContext context )
                {
                    if ( mappings != null )
                    {
                        foreach ( var (field, column, type) in mappings )
                        {
                            if ( string.IsNullOrWhiteSpace( field ) || string.IsNullOrWhiteSpace( column ) )
                            {
                                context.MessageFormatter.AppendArgument( "ValidationMessage", Resources.CoreErrorMessages.DbUpsertMissingElementName );
                                return false;
                            }
                        }
                    }

                    return true;
                }

                bool HaveAtLeastOneKey( DbUpsert transformation, IEnumerable<Mapping> mappings, PropertyValidatorContext context )
                {
                    if ( mappings != null )
                    {
                        foreach ( var (field, column, type) in mappings )
                        {
                            if ( type == ColumnType.Key )
                            {
                                return true;
                            }
                        }
                    }

                    context.MessageFormatter.AppendArgument( "ValidationMessage", Resources.CoreErrorMessages.DbUpsertMissingKey );
                    return false;
                }

                RuleFor( _ => _.ConnectionInfo ).NotNull();
                RuleFor( _ => _.Mappings ).NotNull().NotEmpty().Must( HaveDefinedFieldNames ).Must( HaveAtLeastOneKey );
                RuleFor( _ => _.Table ).NotNull().NotWhiteSpace();
                RuleFor( _ => _.SqlHelper ).NotNull();
                RuleFor( _ => _.DuplicateKeyEventMessage ).NotNull();
                RuleFor( _ => _.OnInserted ).NotNull();
                RuleFor( _ => _.OnUnchanged ).NotNull();
                RuleFor( _ => _.OnUpdated ).NotNull();
            }
        }
    }
}
