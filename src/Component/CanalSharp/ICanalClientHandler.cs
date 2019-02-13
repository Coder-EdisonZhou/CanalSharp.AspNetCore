using System;

namespace CanalSharp.AspNetCore.CanalSharp
{
    public interface ICanalClientHandler : IDisposable
    {
        void Start();

        void Stop();
    }
}
