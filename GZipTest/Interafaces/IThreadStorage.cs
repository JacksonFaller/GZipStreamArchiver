using System;

namespace GZipTest.Interafaces
{
    public interface IThreadStorage<T> : IDisposable
    {
        void EnqueueTask(ThreadTask<T> task);
    }
}