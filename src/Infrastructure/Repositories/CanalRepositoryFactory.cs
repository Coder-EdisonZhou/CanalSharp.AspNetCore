using CanalSharp.AspNetCore.Infrastructure.Enums;

namespace CanalSharp.AspNetCore.Infrastructure
{
    public static class CanalRepositoryFactory
    {
        public static ICanalRepository GetCanalRepositoryInstance(OutputOptions options)
        {
            ICanalRepository canalRepository = null;

            switch (options.Output)
            {
                case OutputEnum.MySql:
                    canalRepository = new MySqlCanalRepository(options as MySqlOutputOptions);
                    break;
                case OutputEnum.Mongo:
                    canalRepository = new MongoCanalRepository(options as MongoOutputOptions);
                    break;
            }

            return canalRepository;
        }
    }
}
