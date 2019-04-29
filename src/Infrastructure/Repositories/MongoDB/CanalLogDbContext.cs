using MongoDB.Driver;

namespace CanalSharp.AspNetCore.Infrastructure
{
    public class CanalLogDbContext: MongoDbContext
    {
        public CanalLogDbContext(MongoOutputOptions outputOptions)
            : base(outputOptions)
        {
        }

        public IMongoCollection<ChangeLog> ChangeLogs
        {
            get
            {
                return mongoDatabase.GetCollection<ChangeLog>("changelogs");
            }
        }
    }
}
