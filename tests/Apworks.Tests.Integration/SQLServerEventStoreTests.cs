﻿using Apworks.Events;
using Apworks.EventStore.AdoNet;
using Apworks.EventStore.SQLServer;
using Apworks.KeyGeneration;
using Apworks.Serialization.Json;
using Apworks.Tests.Integration.Fixtures;
using Apworks.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace Apworks.Tests.Integration
{
    public class SQLServerEventStoreTests : IClassFixture<SQLServerFixture>, IDisposable
    {
        private readonly SQLServerFixture fixture;
       
        public SQLServerEventStoreTests(SQLServerFixture fixture)
        {
            Monitor.Enter(SQLServerFixture.locker);
            this.fixture = fixture;
        }

        public void Dispose()
        {
            this.fixture.ClearTables();
            Monitor.Exit(SQLServerFixture.locker);
        }

        [Fact]
        public void SaveEventsTest()
        {
            var aggregateRootId = Guid.NewGuid();
            var employee = new Employee(aggregateRootId);

            var event1 = new NameChangedEvent("daxnet");
            var event2 = new TitleChangedEvent("title");
            var event3 = new RegisteredEvent();

            event1.AttachTo(employee);
            event2.AttachTo(employee);
            event3.AttachTo(employee);

            var storeConfig = new AdoNetEventStoreConfiguration(SQLServerFixture.ConnectionString, new GuidKeyGenerator());
            var payloadSerializer = new ObjectJsonSerializer();
            var store = new SqlServerEventStore(storeConfig, payloadSerializer);
            store.Save(new List<DomainEvent>
            {
                event1,
                event2,
                event3
            });
        }

        [Fact]
        public void LoadEventsTest()
        {
            var aggregateRootId = Guid.NewGuid();
            var employee = new Employee(aggregateRootId);

            var event1 = new NameChangedEvent("daxnet");
            var event2 = new TitleChangedEvent("title");
            var event3 = new RegisteredEvent();

            event1.AttachTo(employee);
            event2.AttachTo(employee);
            event3.AttachTo(employee);

            var storeConfig = new AdoNetEventStoreConfiguration(SQLServerFixture.ConnectionString, new GuidKeyGenerator());
            var payloadSerializer = new ObjectJsonSerializer();
            var store = new SqlServerEventStore(storeConfig, payloadSerializer);
            store.Save(new List<DomainEvent>
            {
                event1,
                event2,
                event3
            });

            var events = store.Load<Guid>(typeof(Employee).AssemblyQualifiedName, aggregateRootId);
            Assert.Equal(3, events.Count());
        }

        [Fact]
        public void LoadMinimalSequenceEventsTest()
        {
            var aggregateRootId = Guid.NewGuid();
            var employee = new Employee(aggregateRootId);

            var event1 = new NameChangedEvent("daxnet") { Sequence = 1 };
            var event2 = new TitleChangedEvent("title") { Sequence = 2 };
            var event3 = new RegisteredEvent() { Sequence = 3 };

            event1.AttachTo(employee);
            event2.AttachTo(employee);
            event3.AttachTo(employee);

            var storeConfig = new AdoNetEventStoreConfiguration(SQLServerFixture.ConnectionString, new GuidKeyGenerator());
            var payloadSerializer = new ObjectJsonSerializer();
            var store = new SqlServerEventStore(storeConfig, payloadSerializer);
            store.Save(new List<DomainEvent>
            {
                event1,
                event2,
                event3
            });

            var events = store.Load<Guid>(typeof(Employee).AssemblyQualifiedName, aggregateRootId, 2).ToList();
            Assert.Equal(2, events.Count);
            Assert.IsType(typeof(TitleChangedEvent), events[0]);
            Assert.IsType(typeof(RegisteredEvent), events[1]);
        }

        [Fact]
        public void LoadMaximumSequenceEventsTest()
        {
            var aggregateRootId = Guid.NewGuid();
            var employee = new Employee(aggregateRootId);

            var event1 = new NameChangedEvent("daxnet") { Sequence = 1 };
            var event2 = new TitleChangedEvent("title") { Sequence = 2 };
            var event3 = new RegisteredEvent() { Sequence = 3 };

            event1.AttachTo(employee);
            event2.AttachTo(employee);
            event3.AttachTo(employee);

            var storeConfig = new AdoNetEventStoreConfiguration(SQLServerFixture.ConnectionString, new GuidKeyGenerator());
            var payloadSerializer = new ObjectJsonSerializer();
            var store = new SqlServerEventStore(storeConfig, payloadSerializer);
            store.Save(new List<DomainEvent>
            {
                event1,
                event2,
                event3
            });

            var events = store.Load<Guid>(typeof(Employee).AssemblyQualifiedName, aggregateRootId, sequenceMax: 2).ToList();
            Assert.Equal(2, events.Count);
            Assert.IsType(typeof(NameChangedEvent), events[0]);
            Assert.IsType(typeof(TitleChangedEvent), events[1]);
        }

        [Fact]
        public void LoadEventsWithMinMaxSequenceTest()
        {
            var aggregateRootId = Guid.NewGuid();
            var employee = new Employee(aggregateRootId);

            var event1 = new NameChangedEvent("daxnet") { Sequence = 1 };
            var event2 = new TitleChangedEvent("title") { Sequence = 2 };
            var event3 = new RegisteredEvent() { Sequence = 3 };

            event1.AttachTo(employee);
            event2.AttachTo(employee);
            event3.AttachTo(employee);

            var storeConfig = new AdoNetEventStoreConfiguration(SQLServerFixture.ConnectionString, new GuidKeyGenerator());
            var payloadSerializer = new ObjectJsonSerializer();
            var store = new SqlServerEventStore(storeConfig, payloadSerializer);
            store.Save(new List<DomainEvent>
            {
                event1,
                event2,
                event3
            });

            var events = store.Load<Guid>(typeof(Employee).AssemblyQualifiedName, aggregateRootId, 1, 3).ToList();
            Assert.Equal(3, events.Count);
            Assert.IsType(typeof(NameChangedEvent), events[0]);
            Assert.IsType(typeof(TitleChangedEvent), events[1]);
            Assert.IsType(typeof(RegisteredEvent), events[2]);
        }
    }
}
