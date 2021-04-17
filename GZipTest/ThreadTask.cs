using System;

namespace GZipTest
{
    public class ThreadTask<T>
    {
        private readonly Action<T> _action;
        private readonly T _argument;

        public ThreadTask(Action<T> action, T argument)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _argument = argument;
        }

        public void Execute()
        {
            _action(_argument);
        }
    }
}
