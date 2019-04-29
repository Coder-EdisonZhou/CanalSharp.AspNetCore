using System.Collections.Generic;
using System.Threading.Tasks;

namespace CanalSharp.AspNetCore.Infrastructure
{
    public class MongoCanalRepository : ICanalRepository
    {
        private readonly MongoOutputOptions _options;
        private readonly CanalLogDbContext _logDbContext;

        public MongoCanalRepository(MongoOutputOptions options)
        {
            _options = options;
            _logDbContext = new CanalLogDbContext(_options);
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task<bool> SaveChangeHistoriesAsync(List<ChangeLog> changeHistories)
        {
            await _logDbContext.ChangeLogs.InsertManyAsync(changeHistories);
            return true;
        }
    }
}
