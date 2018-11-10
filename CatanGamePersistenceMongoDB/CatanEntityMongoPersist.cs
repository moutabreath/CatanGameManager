using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using MongoDB.Driver;

namespace CatanGamePersistence.MongoDB
{
    public class CatanEntityMongoPersist<T> where T : Entity
    {
        protected MongoClient Client { get; set; }
        protected IMongoDatabase Database { get; set; }

        protected async Task UpdateEntity(T entity, IMongoCollection<T> collection, FilterDefinition<T> filter)
        {
            await collection.ReplaceOneAsync(filter, entity, new UpdateOptions { IsUpsert = true });
        }
    }
}
