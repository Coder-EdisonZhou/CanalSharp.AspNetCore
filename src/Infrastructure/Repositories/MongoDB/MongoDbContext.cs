using MongoDB.Driver;

namespace CanalSharp.AspNetCore.Infrastructure
{
    public class MongoDbContext
    {
        protected readonly IMongoClient mongoClient;
        protected readonly IMongoDatabase mongoDatabase;

        public MongoDbContext(MongoOutputOptions outputOptions)
        {
            mongoClient = new MongoClient(outputOptions.ConnectionString);
            mongoDatabase = mongoClient.GetDatabase(outputOptions.DataBase);
        }
    }
}
