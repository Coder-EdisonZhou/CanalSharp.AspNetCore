using System;

namespace CanalSharp.AspNetCore.CanalSharp
{
    public interface ICanalClientHandler : IDisposable
    {
        void Initialize();

        void Start();

        void Stop();
    }
}
