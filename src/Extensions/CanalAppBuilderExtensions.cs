using CanalSharp.AspNetCore.CanalSharp;
using CanalSharp.AspNetCore.Infrastructure;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;

namespace CanalSharp.AspNetCore.Extensions
{
    public static class CanalAppBuilderExtensions
    {
        public static IApplicationBuilder RegisterCanalSharpClient(this IApplicationBuilder app, IApplicationLifetime lifetime, IConfiguration configuration, ILogger<ICanalClientHandler> canalLogger = null)
        {
            var isEnableCanalClient = Convert.ToBoolean(configuration["Canal:Enabled"] ?? "false");
            if (isEnableCanalClient)
            {
                var mysqlOption = new MySqlOutputOptions()
                {
                    ConnectionString = configuration["Canal:Output:ConnStr"]
                };
                var canalClient = BuildCanalClientHandler(configuration, mysqlOption, canalLogger);
                lifetime.ApplicationStopping.Register(() =>
                {
                    canalClient.Stop();
                });

                InitializeCanalLogTable(mysqlOption, canalLogger);
            }

            return app;
        }

        /// <summary>
        /// 初始化Canal日志数据表
        /// </summary>
        private static void InitializeCanalLogTable(MySqlOutputOptions mysqlOption, ILogger<ICanalClientHandler> canalLogger)
        {
            canalLogger?.LogDebug("Starting to create table canal.logs for mysql database.");

            var ddlSql =
                $@"
CREATE TABLE IF NOT EXISTS `{mysqlOption.TableNamePrefix}.{mysqlOption.TableName}` (
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

            using (var connection = new MySqlConnection(mysqlOption.ConnectionString))
            {
                connection.Execute(ddlSql);
            }

            canalLogger?.LogDebug("Finished to create table canal.logs for mysql database.");
        }

        /// <summary>
        /// 构造CanalClientHandler
        /// </summary>
        private static CanalClientHandler BuildCanalClientHandler(IConfiguration configuration, MySqlOutputOptions mysqlOption, ILogger<ICanalClientHandler> canalLogger)
        {
            var canalClient = new CanalClientHandler(
                new CanalOption()
                {
                    CanalServerIP = configuration["Canal:ServerIP"],
                    CanalServerPort = Convert.ToInt32(configuration["Canal:ServerPort"]),
                    Filter = configuration["Canal:Filter"] ?? string.Empty,
                    Destination = configuration["Canal:Destination"] ?? string.Empty,
                    UserName = configuration["Canal:UserName"] ?? string.Empty,
                    Password = configuration["Canal:Password"] ?? string.Empty,
                    SleepTime = Convert.ToInt32(configuration["Canal:SleepTime"] ?? "2000"),
                    BufferSize = Convert.ToInt32(configuration["Canal:BufferSize"] ?? "1024"),
                    LogSource = configuration["Canal:LogSource"] ?? "[Canal]"
                },
            mysqlOption,
            canalLogger);
            canalClient.Start();

            return canalClient;
        }
    }
}
