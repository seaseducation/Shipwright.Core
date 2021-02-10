// SPDX-License-Identifier: Apache-2.0
// Copyright 2020 TTCO Holding Company, Inc. and Contributors
// Licensed under the Apache License, Version 2.0
// See https://opensource.org/licenses/Apache-2.0 or the LICENSE file in the repository root for the full text of the license.

using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Shipwright.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Shipwright.Dataflows.Transformations.DbUpsert;

namespace Shipwright.Dataflows.Transformations.DbUpsertTests
{
    public class HandlerTests
    {
        private DbUpsert transformation = new DbUpsert { ConnectionInfo = new FakeConnectionInfo(), SqlHelper = new FakeSqlHelper() };
        private IDbConnectionFactory connectionFactory;

        private ITransformationHandler instance() => new Handler( transformation, connectionFactory );

        private readonly Mock<IDbConnectionFactory> mockConnectionFactory;
        private readonly Fixture fixture = new Fixture();

        public HandlerTests()
        {
            mockConnectionFactory = Mockery.Of( out connectionFactory );
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
            public void requires_connectionFactory()
            {
                connectionFactory = null!;
                Assert.Throws<ArgumentNullException>( nameof( connectionFactory ), instance );
            }
        }

        public class ShouldUpdate : HandlerTests
        {
            private readonly Record record = FakeRecord.Create();
            private readonly IDictionary<string, object> current = new Dictionary<string, object>();
            private string sql;
            private IDictionary<string, object> parameters;

            private new Handler instance() => base.instance() as Handler;
            private bool method() => instance().ShouldUpdate( record, current, out sql, out parameters );

            [Fact]
            public async Task returns_false_when_upsert_values_identical()
            {
                var mappings = new List<Mapping>
                {
                    fixture.Create<Mapping>() with { Type = ColumnType.Key },
                    fixture.Create<Mapping>() with { Type = ColumnType.Key },
                    fixture.Create<Mapping>() with { Type = ColumnType.Insert },
                    fixture.Create<Mapping>() with { Type = ColumnType.Upsert },
                    fixture.Create<Mapping>() with { Type = ColumnType.Upsert },
                    fixture.Create<Mapping>() with { Type = ColumnType.Trigger },
                };

                foreach ( var (field, parameter, type) in mappings )
                {
                    record.Data[field] = current[parameter] = fixture.Create<string>();
                }

                var keyMap = new List<ColumnValue>
                {
                    new ColumnValue( mappings[0].Column, "p1", record.Data[mappings[0].Field] ),
                    new ColumnValue( mappings[1].Column, "p2", record.Data[mappings[1].Field]  ),
                };

                var updateMap = new List<ColumnValue>
                {
                    // trigger column should be included in any update, regardless of whether it has changed
                    new ColumnValue( mappings[5].Column, "p5", record.Data[mappings[5].Field] ),
                };

                var expectedSql = transformation.SqlHelper.BuildUpdateSql( transformation.Table, updateMap, keyMap );
                var expectedParameters = keyMap.Union( updateMap ).ToDictionary( _ => _.Parameter, _ => _.Value );

                transformation = transformation with { Mappings = mappings };
                var actual = method();

                Assert.False( actual );
                Assert.Equal( expectedSql, sql );
                Assert.Equal( expectedParameters, parameters );
            }

            [Fact]
            public async Task returns_true_when_basic_upsert_values_different()
            {
                var mappings = new List<Mapping>
                {
                    fixture.Create<Mapping>() with { Type = ColumnType.Key },
                    fixture.Create<Mapping>() with { Type = ColumnType.Key },
                    fixture.Create<Mapping>() with { Type = ColumnType.Insert },
                    fixture.Create<Mapping>() with { Type = ColumnType.Upsert },
                    fixture.Create<Mapping>() with { Type = ColumnType.Upsert },
                    fixture.Create<Mapping>() with { Type = ColumnType.Upsert },
                    fixture.Create<Mapping>() with { Type = ColumnType.Trigger },
                };

                foreach ( var (field, parameter, type) in mappings )
                {
                    record.Data[field] = current[parameter] = fixture.Create<string>();
                }

                var keyMap = new List<ColumnValue>
                {
                    new ColumnValue( mappings[0].Column, "p1", record.Data[mappings[0].Field] ),
                    new ColumnValue( mappings[1].Column, "p2", record.Data[mappings[1].Field]  ),
                };

                var updateMap = new List<ColumnValue>
                {
                    // only the changed columns should be included in an update
                    new ColumnValue( mappings[3].Column, "p3", record.Data[mappings[3].Field] = fixture.Create<string>() ),
                    new ColumnValue( mappings[4].Column, "p4", record.Data[mappings[4].Field] = fixture.Create<string>() ),

                    // trigger column should be included in any update, regardless of whether it has changed
                    new ColumnValue( mappings[6].Column, "p6", record.Data[mappings[6].Field] ),
                };

                var expectedSql = transformation.SqlHelper.BuildUpdateSql( transformation.Table, updateMap, keyMap );
                var expectedParameters = keyMap.Union( updateMap ).ToDictionary( _ => _.Parameter, _ => _.Value );

                transformation = transformation with { Mappings = mappings };
                var actual = method();

                Assert.True( actual );
                Assert.Equal( expectedSql, sql );
                Assert.Equal( expectedParameters, parameters );
            }

            [Fact]
            public async Task returns_true_when_structural_upsert_values_different()
            {
                var mappings = new List<Mapping>
                {
                    fixture.Create<Mapping>() with { Type = ColumnType.Key },
                    fixture.Create<Mapping>() with { Type = ColumnType.Key },
                    fixture.Create<Mapping>() with { Type = ColumnType.Insert },
                    fixture.Create<Mapping>() with { Type = ColumnType.Upsert },
                    fixture.Create<Mapping>() with { Type = ColumnType.Upsert },
                    fixture.Create<Mapping>() with { Type = ColumnType.Upsert },
                    fixture.Create<Mapping>() with { Type = ColumnType.Trigger },
                };

                foreach ( var (field, parameter, type) in mappings )
                {
                    record.Data[field] = current[parameter] = fixture.Create<string>();
                }

                var keyMap = new List<ColumnValue>
                {
                    new ColumnValue( mappings[0].Column, "p1", record.Data[mappings[0].Field] ),
                    new ColumnValue( mappings[1].Column, "p2", record.Data[mappings[1].Field]  ),
                };

                var updateMap = new List<ColumnValue>
                {
                    // only the changed columns should be included in an update
                    new ColumnValue( mappings[3].Column, "p3", record.Data[mappings[3].Field] = fixture.CreateMany<int>() ),
                    new ColumnValue( mappings[4].Column, "p4", record.Data[mappings[4].Field] = fixture.CreateMany<int>() ),

                    // trigger column should be included in any update, regardless of whether it has changed
                    new ColumnValue( mappings[6].Column, "p6", record.Data[mappings[6].Field] ),
                };

                current[mappings[3].Column] = fixture.CreateMany<int>();
                current[mappings[4].Column] = fixture.CreateMany<int>();

                var expectedSql = transformation.SqlHelper.BuildUpdateSql( transformation.Table, updateMap, keyMap );
                var expectedParameters = keyMap.Union( updateMap ).ToDictionary( _ => _.Parameter, _ => _.Value );

                transformation = transformation with { Mappings = mappings };
                var actual = method();

                Assert.True( actual );
                Assert.Equal( expectedSql, sql );
                Assert.Equal( expectedParameters, parameters );
            }
        }

        public class Transform : HandlerTests
        {
            private new Handler instance() => mockHandler.Object;

            private readonly Mock<Handler> mockHandler;

            private Record record = FakeRecord.Create();
            private CancellationToken cancellationToken;
            private Task method() => instance().Transform( record, cancellationToken );

            public Transform()
            {
                mockHandler = new Mock<Handler>( MockBehavior.Loose, transformation, connectionFactory ) { CallBase = true };
                transformation = new DbUpsert
                {
                    ConnectionInfo = new FakeConnectionInfo(),
                    Table = fixture.Create<string>(),
                    SqlHelper = new FakeSqlHelper(),
                    DuplicateKeyEventMessage = count => string.Format( Resources.CoreErrorMessages.DbUpsertKeyNotUnique, count ),
                    Mappings = Enum.GetValues<ColumnType>().Select( type => fixture.Create<Mapping>() with { Type = type } ).ToList(),
                };
            }

            [Fact]
            public async Task requires_record()
            {
                record = null!;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( record ), method );
            }

            [Fact]
            public async Task when_canceled_fails_to_obtain_semaphore()
            {
                cancellationToken = new CancellationToken( true );

                var keys = fixture.Create<IDictionary<string, object>>();
                var select = fixture.Create<string>();
                mockHandler.Setup( _ => _.BuildSelectSql( record, out keys ) ).Returns( select ).Verifiable();

                await Assert.ThrowsAsync<TaskCanceledException>( method );

                mockHandler.Verify( _ => _.BuildSelectSql( record, out keys ), Times.Once() );
                mockHandler.Verify( _ => _.Lock( keys, cancellationToken ), Times.Once() );
            }

            [Fact]
            public async Task when_query_returns_multiple_records_adds_event()
            {
                var keys = fixture.Create<IDictionary<string, object>>();
                var select = fixture.Create<string>();
                mockHandler.Setup( _ => _.BuildSelectSql( record, out keys ) ).Returns( select ).Verifiable();

                var matches = fixture.CreateMany<IDictionary<string, object>>();
                mockHandler.Setup( _ => _.Query( select, keys, cancellationToken ) ).ReturnsAsync( matches ).Verifiable();

                var expectedData = new Dictionary<string, object>( record.Data );
                var expectedEvents = new List<LogEvent> { new LogEvent
                {
                    IsFatal = true,
                    Level = LogLevel.Error,
                    Description = transformation.DuplicateKeyEventMessage( matches.Count() ),
                    Value = new { sql = select, keys }
                }};

                await method();
                Assert.Equal( expectedData, record.Data );
                Assert.Equal( JsonSerializer.Serialize( expectedEvents ), JsonSerializer.Serialize( record.Events ) );
            }

            [Fact]
            public async Task when_query_returns_no_records_performs_insert()
            {
                var keys = fixture.Create<IDictionary<string, object>>();
                var select = fixture.Create<string>();
                mockHandler.Setup( _ => _.BuildSelectSql( record, out keys ) ).Returns( select ).Verifiable();
                mockHandler.Setup( _ => _.Query( select, keys, cancellationToken ) ).ReturnsAsync( Array.Empty<IDictionary<string, object>>() ).Verifiable();

                var insert = fixture.Create<string>();
                var parameters = fixture.Create<IDictionary<string, object>>();
                mockHandler.Setup( _ => _.BuildInsertSql( record, out parameters ) ).Returns( insert );
                mockHandler.Setup( _ => _.Execute( insert, parameters, cancellationToken ) ).Returns( Task.CompletedTask ).Verifiable();

                var expectedData = new Dictionary<string, object>( record.Data );

                await method();
                Assert.Equal( expectedData, record.Data );
                Assert.Empty( record.Events );

                mockHandler.Verify();
                mockHandler.Verify( _ => _.Execute( insert, parameters, cancellationToken ), Times.Once() );
            }

            [Fact]
            public async Task when_query_returns_single_record_without_change_no_op()
            {
                var keys = fixture.Create<IDictionary<string, object>>();
                var select = fixture.Create<string>();
                mockHandler.Setup( _ => _.BuildSelectSql( record, out keys ) ).Returns( select ).Verifiable();

                var matches = fixture.CreateMany<IDictionary<string, object>>( 1 );
                mockHandler.Setup( _ => _.Query( select, keys, cancellationToken ) ).ReturnsAsync( matches ).Verifiable();

                var current = matches.Single();
                var update = fixture.Create<string>();
                var parameters = fixture.Create<IDictionary<string, object>>();
                mockHandler.Setup( _ => _.ShouldUpdate( record, current, out update, out parameters ) ).Returns( false );

                await method();
                mockHandler.Verify();
                mockHandler.Verify( _ => _.Execute( update, parameters, cancellationToken ), Times.Never() );
            }

            [Fact]
            public async Task when_query_returns_single_record_with_changes_updates()
            {
                var keys = fixture.Create<IDictionary<string, object>>();
                var select = fixture.Create<string>();
                mockHandler.Setup( _ => _.BuildSelectSql( record, out keys ) ).Returns( select ).Verifiable();

                var matches = fixture.CreateMany<IDictionary<string, object>>( 1 );
                mockHandler.Setup( _ => _.Query( select, keys, cancellationToken ) ).ReturnsAsync( matches ).Verifiable();

                var current = matches.Single();
                var update = fixture.Create<string>();
                var parameters = fixture.Create<IDictionary<string, object>>();
                mockHandler.Setup( _ => _.ShouldUpdate( record, current, out update, out parameters ) ).Returns( true );
                mockHandler.Setup( _ => _.Execute( update, parameters, cancellationToken ) ).Returns( Task.CompletedTask ).Verifiable();

                await method();
                mockHandler.Verify();
                mockHandler.Verify( _ => _.Execute( update, parameters, cancellationToken ), Times.Once() );
            }
        }
    }
}
