using System;

namespace BirdMessenger.Infrastructure
{
    internal class TemporaryOptionChange : IDisposable
    {
        private readonly Action _disposeAction;

        public TemporaryOptionChange(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction?.Invoke();
        }
    }
}
