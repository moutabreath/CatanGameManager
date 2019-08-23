using CatanGameManager.CommonObjects;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace CatanGamePersistance.MongoDB
{
    public class CatanEntityPersist<T> where T : Entity
    {
        protected MongoClient Client { get; set; }
        protected IMongoDatabase Database { get; set; }

        public async Task UpdateEntity(T entity, IMongoCollection<T> collection)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            await collection.ReplaceOneAsync(game => game.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
        }
    }
}
