﻿using System;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CommonLib.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CatanGamePersistence.MongoDB
{
    public abstract class CatanEntityMongoPersist<T> where T : Entity
    {
        protected readonly ILogger<CatanEntityMongoPersist<T>> _logger;
        protected readonly string _documentName;
        protected MongoClient Client { get; set; }
        protected IMongoDatabase Database { get; set; }
        protected IMongoCollection<T> MongoCollection { get; set; }

        public CatanEntityMongoPersist(ILogger<CatanEntityMongoPersist<T>> logger, IOptions<MongoConfig> options, string documentName)
        {
            _logger = logger;
            _documentName = documentName;
            MongoConfig configuration = options.Value;
            Client = new MongoClient(configuration.MongoConnectionString);
            Database = Client.GetDatabase(configuration.MongoDatabaseName);
            MongoCollection = Database.GetCollection<T>(_documentName);
            InitializeClassMap();
        }

        protected abstract void InitializeClassMap();

        protected async Task<bool> UpdateEntity(T entity, IMongoCollection<T> collection, FilterDefinition<T> filter)
        {
            try
            {
                ReplaceOneResult replaceOneResult = await collection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = true });
                return replaceOneResult.IsAcknowledged;
            }
            catch(Exception ex)
            {
                _logger.LogError($"UpdateEntity Error when updating entity {entity.Id}, filter {filter}", ex);
                return false;
            }
        }
    }
}
