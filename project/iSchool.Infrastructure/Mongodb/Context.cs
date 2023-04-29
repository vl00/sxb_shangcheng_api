using iSchool.Infrastructure.Mongodb.Entities;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Infrastructure.Mongodb
{
    public class Context
    {
        private readonly MongoClient mongoClient;
        private readonly IMongoDatabase database;

        public Context(string connectionString, string databaseName)
        {
            this.mongoClient = new MongoClient(connectionString);
            this.database = mongoClient.GetDatabase(databaseName);
            Map();
        }

        public IMongoCollection<Statistics> Statistics
        {
            get
            {
                return database.GetCollection<Statistics>("statistics");
            }
        }


        /// <summary>
        /// 映射表
        /// </summary>
        private void Map()
        {
            BsonClassMap.RegisterClassMap<Statistics>(cm => { cm.AutoMap(); });
        }

    }
}
