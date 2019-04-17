namespace CanalSharp.AspNetCore.Infrastructure
{
    public class OutputOptions
    {
        public const string DefaultSchema = "canal";
        public const string DefaultTableName = "logs";

        public string TableNamePrefix { get; set; } = DefaultSchema;

        public string TableName { get; set; } = DefaultTableName;
    }
}
