using System;

namespace CanalSharp.AspNetCore.Middleware.CanalSharp
{
    public interface ICanalClientHandler : IDisposable
    {
        void Start();

        void Stop();
    }
}
