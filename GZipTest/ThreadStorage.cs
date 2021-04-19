using GZipTest.Interafaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    class ThreadStorage<T> : IThreadStorage<T>
    {
        private readonly List<Thread> _threads;
        private readonly object _locker = new object();
        private readonly Queue<ThreadTask<T>> _taskQueue = new Queue<ThreadTask<T>>();
        private bool _isDisposed = false;

        public ThreadStorage(int capacity)
        {
            _threads = new List<Thread>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                var thread = new Thread(Work) { IsBackground = true, Name = $"Storage Thread {i}" };
                _threads.Add(thread);
                thread.Start();
            }
        }

        public void EnqueueTask(ThreadTask<T> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (_isDisposed) throw new ObjectDisposedException(nameof(ThreadStorage<T>));
            EnqueueTaskInternal(task);
        }

        private void EnqueueTaskInternal(ThreadTask<T> task)
        {
            lock (_locker)
            {
                _taskQueue.Enqueue(task);
                Monitor.PulseAll(_locker);
            }
        }

        private void Work()
        {
            while (!_isDisposed)
            {
                ThreadTask<T> task;
                lock (_locker)
                {
                    while (_taskQueue.Count == 0) Monitor.Wait(_locker);
                    task = _taskQueue.Dequeue();
                }
                task.Execute();
            }
        }

        private void CleanupTask(T _) => Log.WriteLine($"{Thread.CurrentThread.Name} is finished");

        public void Dispose()
        {
            Log.WriteLine("Thread storage cleanup started");
            _isDisposed = true;
            _threads.ForEach(thread => EnqueueTaskInternal(new ThreadTask<T>(CleanupTask, default)));
            _threads.ForEach(thread => thread.Join());
        }
    }
}
