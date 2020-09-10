// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using FluentValidation;
using Lamar;
using Lamar.Scanning.Conventions;
using System;
using Xunit;

namespace Shipwright.Validation
{
    public class LamarValidationExtensionsTests
    {
        private ServiceRegistry registry = new ServiceRegistry();

        public class AddValidationAdapter : LamarValidationExtensionsTests
        {
            private ServiceRegistry method() => registry.AddValidationAdapter();

            [Fact]
            public void requires_registry()
            {
                registry = null!;
                Assert.Throws<ArgumentNullException>( nameof( registry ), method );
            }

            [Fact]
            public void adds_validation_adapter()
            {
                var actual = method();
                Assert.Same( registry, actual );

                using var container = new Container( registry );
                var instance = container.GetInstance<IValidationAdapter<AddValidationAdapter>>();
                Assert.IsType<ValidationAdapter<AddValidationAdapter>>( instance );
            }
        }

        public class AddValidators : LamarValidationExtensionsTests
        {
            private IAssemblyScanner scanner;
            private IAssemblyScanner method() => scanner.AddValidators();

            public class FakeValidator : AbstractValidator<AddValidators> { }

            [Fact]
            public void requires_scanner()
            {
                scanner = null!;
                Assert.Throws<ArgumentNullException>( nameof( scanner ), method );
            }

            [Fact]
            public void adds_discovered_validator_as_singleton()
            {
                registry.Scan( scanner =>
                {
                    this.scanner = scanner;
                    scanner.AssemblyContainingType<AddValidators>();

                    var actual = method();
                    Assert.Same( this.scanner, actual );
                } );

                using var container = new Container( registry );
                var instance = container.GetInstance<IValidator<AddValidators>>();
                Assert.IsType<FakeValidator>( instance );

                var second = container.GetInstance<IValidator<AddValidators>>();
                Assert.Same( instance, second );
            }
        }
    }
}
