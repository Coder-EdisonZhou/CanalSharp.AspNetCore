namespace CanalSharp.AspNetCore.Infrastructure
{
    public class MongoOutputOptions : OutputOptions
    {
        public string ConnectionString { get; set; }

        public string DataBase { get; set; }
    }
}
