using CanalSharp.AspNetCore.Infrastructure.Enums;

namespace CanalSharp.AspNetCore.CanalSharp
{
    public class CanalOption
    {
        public string CanalServerIP { get; set; }

        public int CanalServerPort { get; set; }

        public string Destination { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Filter { get; set; }

        public int SleepTime { get; set; }

        public int BufferSize { get; set; }

        public string LogSource { get; set; }
    }
}
