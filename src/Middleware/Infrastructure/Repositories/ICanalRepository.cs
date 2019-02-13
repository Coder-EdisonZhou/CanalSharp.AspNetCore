using System.Collections.Generic;
using System.Threading.Tasks;

namespace CanalSharp.AspNetCore.Middleware.Infrastructure
{
    public interface ICanalRepository
    {
        Task<bool> SaveChangeHistoriesAsync(List<ChangeLog> changeHistories);
    }
}
