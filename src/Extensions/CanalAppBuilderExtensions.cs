using CanalSharp.AspNetCore.CanalSharp;
using CanalSharp.AspNetCore.Infrastructure;
using CanalSharp.AspNetCore.Infrastructure.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace CanalSharp.AspNetCore.Extensions
{
    public static class CanalAppBuilderExtensions
    {
        public static IApplicationBuilder UseCanalClient(this IApplicationBuilder app, IConfiguration configuration)
        {
            var isEnableCanalClient = Convert.ToBoolean(configuration["Canal:Enabled"] ?? "false");
            if (isEnableCanalClient)
            {
                var outputOptions = BuildOutputOptions(configuration);
                var logger = app.ApplicationServices.GetService(typeof(ILogger<ICanalClientHandler>)) as ILogger<ICanalClientHandler>;
                var canalClient = BuildCanalClientHandler(configuration, outputOptions, logger);
                canalClient.Initialize();
                canalClient.Start();

                var appLifeTime = app.ApplicationServices.GetService(typeof(IApplicationLifetime)) as IApplicationLifetime;
                appLifeTime.ApplicationStopping.Register(() =>
                {
                    canalClient.Stop();
                });
            }

            return app;
        }

        /// <summary>
        /// 构造OutputOptions
        /// </summary>
        /// <param name="configuration">配置文件</param>
        /// <returns>OutputOptions</returns>
        private static OutputOptions BuildOutputOptions(IConfiguration configuration)
        {
            OutputOptions outputOptions = null;

            // MySql output
            if (configuration["Canal:Output:MySql:ConnStr"] != null)
            {
                outputOptions = new MySqlOutputOptions()
                {
                    ConnectionString = configuration["Canal:Output:MySql:ConnStr"] ?? 
                        throw new ArgumentNullException("[CanalClient] MySql连接字符串不能为空!"),
                    Output = OutputEnum.MySql
                };
            }
            // Mongo output
            if (configuration["Canal:Output:Mongo:ConnStr"] != null)
            {
                outputOptions = new MongoOutputOptions()
                {
                    ConnectionString = configuration["Canal:Output:Mongo:ConnStr"] ??
                        throw new ArgumentNullException("[CanalClient] Mongo连接字符串不能为空!"),
                    DataBase = configuration["Canal:Output:Mongo:DataBase"] ??
                        throw new ArgumentNullException("[CanalClient] Mongo数据库名不能为空!"),
                    Output = OutputEnum.Mongo
                };
            }

            return outputOptions;
        }

        /// <summary>
        /// 构造CanalClientHandler
        /// </summary>
        private static CanalClientHandler BuildCanalClientHandler(IConfiguration configuration, OutputOptions outputOptions, ILogger<ICanalClientHandler> canalLogger)
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
            outputOptions,
            canalLogger);

            return canalClient;
        }
    }
}
