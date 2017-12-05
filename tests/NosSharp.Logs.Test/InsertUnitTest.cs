using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using NosSharp.Logs.Loggers;

namespace NosSharp.Logs.Test
{
    [TestClass]
    public class InsertUnitTest
    {
        private class TestLoggable : AbstractLoggable
        {
            public TestLoggable() : base("TestLoggable")
            {
            }
        }


        private static readonly MongoLogger Logger = new MongoLogger("NosSharpTest");
        private const string CollectionName = "TestInsertLogs";

        [TestMethod]
        public void TestInsertSingleLog()
        {
            Logger.DeleteAll(CollectionName);
            TestLoggable test = new TestLoggable();
            
            Logger.InsertLog(test);
        }

        [TestMethod]
        public void TestInsertMultipleLogs()
        {
            IEnumerable<ILoggable> documents = Enumerable.Range(0, 100).Select(i => new TestLoggable());
            Logger.InsertLogs(documents);
        }
    }
}