using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NosSharp.Logs.Loggers
{
    public class MongoLogger : ILogger
    {
        protected IMongoClient Client;
        protected IMongoDatabase Database;

        public MongoLogger(string database)
        {
            Client = new MongoClient("mongodb://localhost:27017");
            Database = Client.GetDatabase(database);
        }

        public MongoLogger(string connection, string database)
        {
            Client = new MongoClient(connection);
            Database = Client.GetDatabase(database);
        }

        public async void InsertLog(ILoggable loggable)
        {
            if (loggable == null)
            {
                return;
            }

            IMongoCollection<BsonDocument> collection = Database.GetCollection<BsonDocument>(loggable.Collection);

            if (collection == null)
            {
                await Database.CreateCollectionAsync(loggable.Collection);
                collection = Database.GetCollection<BsonDocument>(loggable.Collection);
            }

            await collection.InsertOneAsync(loggable.ToBsonDocument());
        }

        public async void InsertLogs(IEnumerable<ILoggable> abstractLogs)
        {
            if (abstractLogs == null)
            {
                return;
            }

            foreach (IGrouping<string, ILoggable> logs in abstractLogs.GroupBy(s => s.Collection))
            {
                IMongoCollection<BsonDocument> collection = Database.GetCollection<BsonDocument>(logs.Key);

                if (collection == null)
                {
                    await Database.CreateCollectionAsync(logs.Key);
                    collection = Database.GetCollection<BsonDocument>(logs.Key);
                }

                await collection.InsertManyAsync(logs.Select(s => s.ToBsonDocument()));
            }
        }

        public async void DeleteAll(string loggableCategory)
        {
            await Database.DropCollectionAsync(loggableCategory);
        }
    }
}