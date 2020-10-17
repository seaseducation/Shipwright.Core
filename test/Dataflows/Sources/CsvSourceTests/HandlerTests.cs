// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Sources.CsvSource;

namespace Shipwright.Dataflows.Sources.CsvSourceTests
{
    public class HandlerTests
    {
        private ISourceHandler<CsvSource> instance() => new Handler();

        public class Read : HandlerTests
        {
            private CsvSource source = new CsvSource { Path = Guid.NewGuid().ToString(), Settings = CsvConfigurations.CommaSeparatedRfc4180WithHeader( CultureInfo.CurrentCulture ) };
            private Dataflow dataflow = FakeRecord.Fixture().Create<Dataflow>();

            private async Task<IEnumerable<Record>> method()
            {
                var instance = this.instance();
                var records = new List<Record>();

                await foreach ( var record in instance.Read( source, dataflow, default ) )
                {
                    records.Add( record );
                }

                return records;
            }

            [Fact]
            public async Task requires_source()
            {
                source = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( source ), method );
            }

            [Fact]
            public async Task requires_dataflow()
            {
                dataflow = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( dataflow ), method );
            }

            [Fact]
            public async Task throws_when_file_does_not_exist_and_throw_option_true()
            {
                source = source with { ThrowOnFileNotFound = true };
                var ex = await Assert.ThrowsAsync<FileNotFoundException>( method );
                dataflow.Events.Should().ContainEquivalentOf( new LogEvent
                {
                    IsFatal = true,
                    Level = Microsoft.Extensions.Logging.LogLevel.Error,
                    Description = ex.Message,
                    Value = new { source.Path }
                }, options => options.ComparingByMembers<LogEvent>() );
            }

            [Fact]
            public async Task logs_file_does_not_exist_when_throw_option_false()
            {
                source = source with { ThrowOnFileNotFound = false };
                await method();
                dataflow.Events.Should().NotBeEmpty();
            }

            [Fact]
            public async Task throws_when_field_count_changes_and_detection_on_and_throw_option_true()
            {
                source = source with
                {
                    Path = "Dataflows/Sources/CsvSourceTests/InvalidFieldCountChange.csv",
                    Settings = CsvConfigurations.CommaSeparatedRfc4180WithHeader( CultureInfo.CurrentCulture ),
                    ThrowOnBadData = true,
                };

                var ex = await Assert.ThrowsAsync<BadDataException>( method );
                Assert.Equal( 3, ex.ReadingContext.RawRow );

                dataflow.Events.Should().ContainSingle().Subject.Should().BeEquivalentTo( new LogEvent
                {
                    IsFatal = true,
                    Level = Microsoft.Extensions.Logging.LogLevel.Error,
                    Description = ex.Message,
                    Value = new { Line = 3, source.Path },
                }, options => options.ComparingByMembers<LogEvent>() );
            }

            [Fact]
            public async Task logs_field_count_changes_and_detection_on_and_throw_option_false()
            {
                source = source with
                {
                    Path = "Dataflows/Sources/CsvSourceTests/InvalidFieldCountChange.csv",
                    Settings = CsvConfigurations.CommaSeparatedRfc4180WithHeader( CultureInfo.CurrentCulture ),
                    ThrowOnBadData = false,
                };

                await method();
                dataflow.Events.Should().NotBeEmpty();
            }

            [Fact]
            public async Task throws_when_unescaped_quote_and_throw_option_true()
            {
                source = source with
                {
                    Path = "Dataflows/Sources/CsvSourceTests/InvalidUnescapedQuote.csv",
                    Settings = CsvConfigurations.CommaSeparatedRfc4180WithHeader( CultureInfo.CurrentCulture ),
                    ThrowOnBadData = true,
                };

                var ex = await Assert.ThrowsAsync<BadDataException>( method );
                Assert.Equal( 2, ex.ReadingContext.RawRow );

                dataflow.Events.Should().ContainSingle().Subject.Should().BeEquivalentTo( new LogEvent
                {
                    IsFatal = true,
                    Level = Microsoft.Extensions.Logging.LogLevel.Error,
                    Description = string.Format( Resources.CsvHelperMessages.UnescapedQuote, source.Settings.Quote ),
                    Value = new { Line = 2, source.Path },
                }, options => options.ComparingByMembers<LogEvent>() );
            }

            [Fact]
            public async Task logs_unescaped_quote_and_throw_option_false()
            {
                source = source with
                {
                    Path = "Dataflows/Sources/CsvSourceTests/InvalidUnescapedQuote.csv",
                    Settings = CsvConfigurations.CommaSeparatedRfc4180WithHeader( CultureInfo.CurrentCulture ),
                    ThrowOnBadData = false,
                };

                await method();

                dataflow.Events.Should().ContainSingle().Subject.Should().BeEquivalentTo( new LogEvent
                {
                    IsFatal = true,
                    Level = Microsoft.Extensions.Logging.LogLevel.Error,
                    Description = string.Format( Resources.CsvHelperMessages.UnescapedQuote, source.Settings.Quote ),
                    Value = new { Line = 2, source.Path },
                }, options => options.ComparingByMembers<LogEvent>() );
            }

            [Fact]
            public async Task throws_when_duplicate_header_and_throw_option_true()
            {
                source = source with
                {
                    Path = "Dataflows/Sources/CsvSourceTests/InvalidDuplicateHeader.csv",
                    Settings = CsvConfigurations.CommaSeparatedRfc4180WithHeader( CultureInfo.CurrentCulture ),
                    ThrowOnBadData = true,
                };

                var ex = await Assert.ThrowsAsync<BadDataException>( method );
                Assert.Equal( 2, ex.ReadingContext.RawRow );

                dataflow.Events.Should().ContainSingle().Subject.Should().BeEquivalentTo( new LogEvent
                {
                    IsFatal = true,
                    Level = Microsoft.Extensions.Logging.LogLevel.Error,
                    Description = string.Format( Resources.CsvHelperMessages.DuplicateHeaderName, "A" ),
                    Value = new { Line = 2 },
                }, options => options.ComparingByMembers<LogEvent>() );
            }

            [Fact]
            public async Task logs_duplicate_header_and_throw_option_false()
            {
                source = source with
                {
                    Path = "Dataflows/Sources/CsvSourceTests/InvalidDuplicateHeader.csv",
                    Settings = CsvConfigurations.CommaSeparatedRfc4180WithHeader( CultureInfo.CurrentCulture ),
                    ThrowOnBadData = false,
                };

                await method();

                dataflow.Events.Should().ContainSingle().Subject.Should().BeEquivalentTo( new LogEvent
                {
                    IsFatal = true,
                    Level = Microsoft.Extensions.Logging.LogLevel.Error,
                    Description = string.Format( Resources.CsvHelperMessages.DuplicateHeaderName, "A" ),
                    Value = new { Line = 2 },
                }, options => options.ComparingByMembers<LogEvent>() );
            }

            [Theory]
            [InlineData( TrimOptions.Trim, null )]
            [InlineData( TrimOptions.None, " " )]
            public async Task returns_records_with_first_line_header_names_when_header_used( TrimOptions trimOptions, string whitespace )
            {
                source = new CsvSource
                {
                    Path = "Dataflows/Sources/CsvSourceTests/ValidFile.csv",
                    Settings = new CsvConfiguration( CultureInfo.CurrentCulture ) { HasHeaderRecord = true, TrimOptions = trimOptions },
                };

                var expected = new List<Record>
                {
                    new Record( dataflow, source, new Dictionary<string,object> { { "A", "x" }, { "B", "y" }, { "C", "z" } }, 2 ),
                    new Record( dataflow, source, new Dictionary<string,object> { { "A", "1" }, { "B", "2" }, { "C", "\"3\"" } }, 3 ),

                    // whitespace should be null when trimming
                    new Record( dataflow, source, new Dictionary<string,object> { { "A", "m" }, { "B", whitespace }, { "C", "n" } }, 4 ),

                    // blank should always be null
                    new Record( dataflow, source, new Dictionary<string,object> { { "A", "r" }, { "B", null }, { "C", "t" } }, 5 ),
                };

                var actual = (await method()).ToArray();
                Assert.Equal( expected.Count, actual.Count() );

                for ( var i = 0; i < expected.Count; i++ )
                {
                    actual[i].Dataflow.Should().BeSameAs( expected[i].Dataflow );
                    actual[i].Source.Should().BeSameAs( expected[i].Source );
                    actual[i].Data.Should().BeEquivalentTo( expected[i].Data );
                    actual[i].Origin.Should().BeEquivalentTo( expected[i].Origin );
                    actual[i].Position.Should().Be( expected[i].Position );
                    actual[i].Events.Should().BeEquivalentTo( expected[i].Events );
                }
            }

            [Theory]
            [InlineData( TrimOptions.Trim, null )]
            [InlineData( TrimOptions.None, " " )]
            public async Task returns_records_with_positional_header_names_when_header_not_used( TrimOptions trimOptions, string whitespace )
            {
                source = new CsvSource
                {
                    Path = "Dataflows/Sources/CsvSourceTests/ValidFile.csv",
                    Settings = new CsvConfiguration( CultureInfo.CurrentCulture ) { HasHeaderRecord = false, TrimOptions = trimOptions },
                };

                var expected = new List<Record>
                {
                    new Record( dataflow, source, new Dictionary<string,object> { { "Field_0", "A" }, { "Field_1", "B" }, { "Field_2", "C" } }, 1 ),
                    new Record( dataflow, source, new Dictionary<string,object> { { "Field_0", "x" }, { "Field_1", "y" }, { "Field_2", "z" } }, 2 ),
                    new Record( dataflow, source, new Dictionary<string,object> { { "Field_0", "1" }, { "Field_1", "2" }, { "Field_2", "\"3\"" } }, 3 ),

                    // whitespace should be null when trimming
                    new Record( dataflow, source, new Dictionary<string,object> { { "Field_0", "m" }, { "Field_1", whitespace }, { "Field_2", "n" } }, 4 ),

                    // blank should always be null
                    new Record( dataflow, source, new Dictionary<string,object> { { "Field_0", "r" }, { "Field_1", null }, { "Field_2", "t" } }, 5 ),
                };

                var actual = (await method()).ToArray();
                Assert.Equal( expected.Count, actual.Count() );

                for ( var i = 0; i < expected.Count; i++ )
                {
                    actual[i].Dataflow.Should().BeSameAs( expected[i].Dataflow );
                    actual[i].Source.Should().BeSameAs( expected[i].Source );
                    actual[i].Data.Should().BeEquivalentTo( expected[i].Data );
                    actual[i].Origin.Should().BeEquivalentTo( expected[i].Origin );
                    actual[i].Position.Should().Be( expected[i].Position );
                    actual[i].Events.Should().BeEquivalentTo( expected[i].Events );
                }
            }


        }
    }
}
