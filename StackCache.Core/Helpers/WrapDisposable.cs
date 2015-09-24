namespace StackCache.Core
{
    using System;

    internal class WrapDisposable : IDisposable
    {
        private readonly IDisposable _localDispose, _remoteDispose;

        public WrapDisposable(IDisposable localDispose, IDisposable remoteDispose)
        {
            if (localDispose == null) throw new ArgumentNullException(nameof(localDispose));
            if (remoteDispose == null) throw new ArgumentNullException(nameof(remoteDispose));
            this._localDispose = localDispose;
            this._remoteDispose = remoteDispose;
        }

        public void Dispose()
        {
            this._remoteDispose.Dispose();
            this._localDispose.Dispose();
        }
    }
}