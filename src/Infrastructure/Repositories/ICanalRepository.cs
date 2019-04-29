using System.Collections.Generic;
using System.Threading.Tasks;

namespace CanalSharp.AspNetCore.Infrastructure
{
    public interface ICanalRepository
    {
        Task InitializeAsync();

        Task<bool> SaveChangeHistoriesAsync(List<ChangeLog> changeHistories);
    }
}
