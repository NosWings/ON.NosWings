using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using OpenNos.Logger;

namespace NosSharp.Logs.Test
{
    [TestClass]
    public class InsertUnitTest
    {
        private static readonly NosSharpLogger _logger = new NosSharpLogger("NosSharpTest");
        private const string CollectionName = "TestInsertLogs";
        readonly IMongoCollection<BsonDocument> _collectionByName = _logger.GetCollectionByName(CollectionName);

        [TestMethod]
        public void TestInsertSingleLog()
        {
            _collectionByName.Database.DropCollection(CollectionName);

            BsonDocument testLog1 = new BsonDocument
            {
                {"Character", "Blowa"},
                {"ChatType", "World"},
                {"Data", "Hello World - NosSharp"},
            };
            _logger.InsertLog(testLog1, CollectionName);
        }

        [TestMethod]
        public void TestInsertMultipleLogs()
        {
            NosSharpLogger logger = new NosSharpLogger("NosSharpTest");

            IMongoCollection<BsonDocument> collectionByName = logger.GetCollectionByName(CollectionName);

            IEnumerable<BsonDocument> documents = Enumerable.Range(0, 100).Select(i => new BsonDocument
            {
                {"Character", $"Blowa - {i}"},
                {"ChatType", "World"},
                {"Data", "Hello World - NosSharp"},
            });
            logger.InsertLogs(documents, CollectionName);
        }
    }
}