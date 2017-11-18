using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OpenNos.Logger
{
    public abstract class NWLogger
    {
        protected static IMongoClient Client;
        protected static IMongoDatabase Database;

        private NWLogger(string database)
        {
            Client = new MongoClient();
            Database = Client.GetDatabase(database);
        }

        public async void InsertLog(BsonDocument log, string collectionName)
        {
            IMongoCollection<BsonDocument> collection = Database.GetCollection<BsonDocument>(collectionName);
            await collection.InsertOneAsync(log);
        }

        public async void InsertLogs(IEnumerable<BsonDocument> logs, string collectionName)
        {
            IMongoCollection<BsonDocument> collection = Database.GetCollection<BsonDocument>(collectionName);
            await collection.InsertManyAsync(logs);
        }
    }
}
