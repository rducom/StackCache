namespace StackCache.Core.Locking
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class AsyncExclusive<T>
    {
        private readonly Func<Task<T>> _task;
        private readonly AsyncLock _lock = new AsyncLock();
        private Task<T> _singleExecute;

        public AsyncExclusive(Func<Task<T>> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            this._task = task;
        }
         
        public async Task<T> Exclusive()
        {
            if (this._singleExecute != null)
                return this._singleExecute.Result;

            using (await this._lock.LockAsync())
            {
                if (this._singleExecute != null)
                    return this._singleExecute.Result;
                if (this._singleExecute == null)
                    this._singleExecute = this._task();
                return await this._singleExecute;
            }
        }
         
    }
}