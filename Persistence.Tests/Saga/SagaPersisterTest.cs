using System;
using NServiceBus.SqlPersistence;
using NUnit.Framework;
using ObjectApproval;

[TestFixture]
#if (!DEBUG)
[Explicit]
#endif
public class SagaPersisterTest
{
    [Test]
    public void Complete()
    {
        var sagaDefinition = new SagaDefinition
        {
            Name = SagaTableNameBuilder.GetTableSuffix(typeof(MySagaData))
        };
        using (var testDatabase = new SagaDatabase(sagaDefinition))
        {
            var persister = testDatabase.Persister;
            var id = Guid.NewGuid();
            var sagaData = new MySagaData
            {
                Id = id,
                OriginalMessageId = "theOriginalMessageId",
                Originator = "theOriginator",
                Property = "theProperty"
            };
            persister.Save(sagaData, null, null).Await();
            persister.Complete(sagaData, null).Await();
            Assert.IsNull(persister.Get<MySagaData>(id, null).Result);
        }
    }

    [Test]
    public void GetById()
    {
        var sagaDefinition = new SagaDefinition
        {
            Name = SagaTableNameBuilder.GetTableSuffix(typeof(MySagaData))
        };
        using (var testDatabase = new SagaDatabase(sagaDefinition))
        {
            var persister = testDatabase.Persister;
            var id = Guid.NewGuid();
            persister.Save(new MySagaData
            {
                Id = id,
                OriginalMessageId = "theOriginalMessageId",
                Originator = "theOriginator",
                Property = "theProperty"
            },null,null).Await();
            var result = persister.Get<MySagaData>(id,null).Result;
            ObjectApprover.VerifyWithJson(result, s => s.Replace(id.ToString(), "theSagaId"));
        }
    }

    public class MySagaData : XmlSagaData
    {
        public string Property { get; set; }
    }

    [Test]
    public void GetByMapping()
    {
        var sagaDefinition = new SagaDefinition
        {
            Name = SagaTableNameBuilder.GetTableSuffix(typeof(MySagaData)),
            CorrelationMember = "Property"
        };
        using (var testDatabase = new SagaDatabase(sagaDefinition))
        {
            var persister = testDatabase.Persister;
            var id = Guid.NewGuid();
            persister.Save(new MySagaData
            {
                Id = id,
                OriginalMessageId = "theOriginalMessageId",
                Originator = "theOriginator",
                Property = "theProperty"
            }, null, null).Await();
            var result = persister.Get<MySagaData>("Property", "theProperty",null).Result;
            ObjectApprover.VerifyWithJson(result, s => s.Replace(id.ToString(), "theSagaId"));
        }
    }
    [Test]
    //TODO:
    public void SaveDuplicateShouldThrow()
    {
        var sagaDefinition = new SagaDefinition
        {
            Name = SagaTableNameBuilder.GetTableSuffix(typeof(MySagaData)),
            CorrelationMember = "Property"
        };
        using (var testDatabase = new SagaDatabase(sagaDefinition))
        {
            var persister = testDatabase.Persister;
            persister.Save(new MySagaData
            {
                Id = Guid.NewGuid(),
                OriginalMessageId = "theOriginalMessageId",
                Originator = "theOriginator",
                Property = "theProperty"
            },null,null).Await();
            persister.Save(new MySagaData
            {
                Id = Guid.NewGuid(),
                OriginalMessageId = "theOriginalMessageId",
                Originator = "theOriginator",
                Property = "theProperty"
            },null,null).Await();
        }
    }

}