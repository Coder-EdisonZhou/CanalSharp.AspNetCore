using CanalSharp.AspNetCore.Infrastructure.Enums;

namespace CanalSharp.AspNetCore.Infrastructure
{
    public class OutputOptions
    {
        public const string DefaultSchema = "canal";
        public const string DefaultTableName = "logs";
        public const OutputEnum DefaultOutput = OutputEnum.MySql;

        public virtual string TableNamePrefix { get; set; } = DefaultSchema;

        public virtual string TableName { get; set; } = DefaultTableName;

        public virtual OutputEnum Output { get; set; } = DefaultOutput;
    }
}
