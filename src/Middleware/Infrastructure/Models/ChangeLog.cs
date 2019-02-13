using System;

namespace CanalSharp.AspNetCore.Middleware.Infrastructure
{
    public class ChangeLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public string EventType { get; set; }

        public string ColumnName { get; set; }

        public string PreviousValue { get; set; }

        public string CurrentValue { get; set; }

        public DateTime ExecuteTime { get; set; }
    }
}
