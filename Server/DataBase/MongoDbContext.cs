using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configurations;
using DataBase.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DataBase
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> options)
        {
            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name = null)
        {
            var collectionName = name ?? typeof(T).Name + "s";
            return _database.GetCollection<T>(collectionName);
        }
    }
}
