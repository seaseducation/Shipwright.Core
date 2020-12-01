// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.CsvOutput;

namespace Shipwright.Dataflows.Transformations.CsvOutputTests
{
    public class HandlerTests
    {
        CsvOutput transformation = new CsvOutput();
        IHelper helper;
        ITransformationHandler instance() => new Handler( transformation, helper );

        readonly Mock<IHelper> mockHelper;

        public HandlerTests()
        {
            mockHelper = Mockery.Of( out helper );
        }

        public class Constructor : HandlerTests
        {
            [Fact]
            public void requires_transformation()
            {
                transformation = null!;
                Assert.Throws<ArgumentNullException>( nameof( transformation ), instance );
            }

            [Fact]
            public void requires_helper()
            {
                helper = null!;
                Assert.Throws<ArgumentNullException>( nameof( helper ), instance );
            }
        }

        public class DisposeAsync : HandlerTests
        {
            async Task method() => await instance().DisposeAsync();

            [Fact]
            public async Task disposes_helper()
            {
                mockHelper.Setup( _ => _.DisposeAsync() ).Returns( ValueTask.CompletedTask );

                await method();
                mockHelper.Verify( _ => _.DisposeAsync(), Times.Once );
            }
        }

        public class Transform : HandlerTests
        {
            Record record = FakeRecord.Create();
            async Task method()
            {
                mockHelper.Setup( _ => _.DisposeAsync() ).Returns( ValueTask.CompletedTask );
                await using var instance = base.instance();
                await instance.Transform( record, default );
            }

            [Fact]
            public async Task requires_record()
            {
                record = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( record ), method );
            }

            [Fact]
            public async Task writes_configured_and_present_record_fields_to_helper()
            {
                var fixture = new Fixture();
                var expected = new List<object>();
                var actual = new List<object>();

                // add standard values
                for ( var i = 0; i < 3; i++ )
                {
                    var field = fixture.Create<string>();
                    var column = fixture.Create<string>();
                    var value = fixture.Create<string>();
                    expected.Add( value );
                    transformation.Output.Add( (field, column) );
                    record.Data[field] = value;
                }

                mockHelper.Setup( _ => _.WriteField( Capture.In( actual ) ) );
                mockHelper.Setup( _ => _.NextRecord() );
                await method();

                Assert.Equal( expected, actual );
                mockHelper.Verify( _ => _.NextRecord(), Times.Once );
                mockHelper.Verify( _ => _.DisposeAsync(), Times.Once );
            }

            [Fact]
            public async Task when_field_value_null_writes_null()
            {
                var fixture = new Fixture();
                var expected = new List<object>();
                var actual = new List<object>();

                // add standard value
                {
                    var field = fixture.Create<string>();
                    var column = fixture.Create<string>();
                    var value = fixture.Create<string>();
                    expected.Add( value );
                    transformation.Output.Add( (field, column) );
                    record.Data[field] = value;
                }

                // add null value
                {
                    var field = fixture.Create<string>();
                    var column = fixture.Create<string>();
                    expected.Add( null );
                    transformation.Output.Add( (field, column) );
                    record.Data[field] = null;
                }

                mockHelper.Setup( _ => _.WriteField( Capture.In( actual ) ) );
                mockHelper.Setup( _ => _.NextRecord() );
                await method();

                Assert.Equal( expected, actual );
                mockHelper.Verify( _ => _.NextRecord(), Times.Once );
                mockHelper.Verify( _ => _.DisposeAsync(), Times.Once );
            }

            [Fact]
            public async Task when_field_missing_writes_null()
            {
                var fixture = new Fixture();
                var expected = new List<object>();
                var actual = new List<object>();

                // add standard value
                {
                    var field = fixture.Create<string>();
                    var column = fixture.Create<string>();
                    var value = fixture.Create<string>();
                    expected.Add( value );
                    transformation.Output.Add( (field, column) );
                    record.Data[field] = value;
                }

                // add missing value
                {
                    var field = fixture.Create<string>();
                    var column = fixture.Create<string>();
                    expected.Add( null );
                    transformation.Output.Add( (field, column) );
                    record.Data.Remove( field );
                }

                mockHelper.Setup( _ => _.WriteField( Capture.In( actual ) ) );
                mockHelper.Setup( _ => _.NextRecord() );
                await method();

                Assert.Equal( expected, actual );
                mockHelper.Verify( _ => _.NextRecord(), Times.Once );
                mockHelper.Verify( _ => _.DisposeAsync(), Times.Once );
            }

            [Fact]
            public async Task when_column_unmapped_writes_null()
            {
                var fixture = new Fixture();
                var expected = new List<object>();
                var actual = new List<object>();

                // add standard value
                {
                    var field = fixture.Create<string>();
                    var column = fixture.Create<string>();
                    var value = fixture.Create<string>();
                    expected.Add( value );
                    transformation.Output.Add( (field, column) );
                    record.Data[field] = value;
                }

                // add unmapped column value
                {
                    var column = fixture.Create<string>();
                    expected.Add( null );
                    transformation.Output.Add( (null, column) );
                }

                mockHelper.Setup( _ => _.WriteField( Capture.In( actual ) ) );
                mockHelper.Setup( _ => _.NextRecord() );
                await method();

                Assert.Equal( expected, actual );
                mockHelper.Verify( _ => _.NextRecord(), Times.Once );
                mockHelper.Verify( _ => _.DisposeAsync(), Times.Once );
            }
        }
    }
}
