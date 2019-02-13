using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

namespace CanalSharp.AspNetCore.Middleware.Infrastructure
{
    public class MySqlCanalRepository : ICanalRepository
    {
        private readonly MySqlOutputOptions _options;

        public MySqlCanalRepository(MySqlOutputOptions options)
        {
            _options = options;
        }

        public async Task<bool> SaveChangeHistoriesAsync(List<ChangeLog> changeHistories)
        {
            if (changeHistories == null || changeHistories.Count == 0)
            {
                return true;
            }

            using (var conn = new MySqlConnection(_options.ConnectionString))
            {
                var sql = $@"INSERT INTO `{_options.TableNamePrefix}.{_options.TableName}`
(`Id`,`SchemaName`,`TableName`,`EventType`,`ColumnName`,`PreviousValue`,`CurrentValue`,`ExecuteTime`) 
VALUES(@Id, @SchemaName, @TableName, @EventType, @ColumnName, @PreviousValue, @CurrentValue, @ExecuteTime)";
                return await conn.ExecuteAsync(sql, changeHistories) > 0;
            }
        }
    }
}
