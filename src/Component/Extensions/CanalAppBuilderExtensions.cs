using CanalSharp.AspNetCore.CanalSharp;
using CanalSharp.AspNetCore.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
                new MySqlOutputOptions()
                {
                    ConnectionString = configuration["Canal:Output:ConnStr"]
                },
                canalLogger);
                canalClient.Start();

                lifetime.ApplicationStopping.Register(() =>
                {
                    canalClient.Stop();
                });
            }

            return app;
        }
    }
}
