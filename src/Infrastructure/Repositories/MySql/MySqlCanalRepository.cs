using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

namespace CanalSharp.AspNetCore.Infrastructure
{
    public class MySqlCanalRepository : ICanalRepository
    {
        private readonly MySqlOutputOptions _options;

        public MySqlCanalRepository(MySqlOutputOptions options)
        {
            _options = options;
        }

        public async Task InitializeAsync()
        {
            var ddlSql =
                $@"
CREATE TABLE IF NOT EXISTS `{_options.TableNamePrefix}.{_options.TableName}` (
`Id` varchar(128) NOT NULL COMMENT 'Id',
`SchemaName` varchar(50) DEFAULT NULL COMMENT '数据库名称',
`TableName` varchar(50) DEFAULT NULL COMMENT '表名',
`EventType` varchar(50) DEFAULT NULL COMMENT '事件类型',
`ColumnName` varchar(50) DEFAULT NULL COMMENT '列名',
`PreviousValue` text COMMENT '变更前的值',
`CurrentValue` text COMMENT '变更后的值',
`ExecuteTime` timestamp NULL DEFAULT CURRENT_TIMESTAMP COMMENT '变更时间',
PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='变更日志记录表';";

            using (var conn = new MySqlConnection((_options as MySqlOutputOptions).ConnectionString))
            {
                await conn.ExecuteAsync(ddlSql);
            }
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
