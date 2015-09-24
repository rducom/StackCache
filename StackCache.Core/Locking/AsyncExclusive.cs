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

        public async Task<T> ExclusiveAsync_v1()
        {
            if (this._singleExecute != null)
                return this._singleExecute.Result;

            using (await this._lock.LockAsync())
            {
                if (this._singleExecute != null)
                    return this._singleExecute.Result;
                Task<T> toAwaitOnce = this._task();
                await toAwaitOnce;
                this._singleExecute = toAwaitOnce;
            }

            return this._singleExecute.Result;
        }


        public async Task<T> ExclusiveAsync_v2()
        {
            if (this._singleExecute != null)
                return this._singleExecute.Result;

            using (await this._lock.LockAsync())
            {
                if (this._singleExecute == null)
                    this._singleExecute = this._task();
                if (this._singleExecute.IsCompleted)
                    return this._singleExecute.Result;
                return await this._singleExecute;
            }
        }


        public async Task<T> ExclusiveAsync_v4()
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

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public Task<T> ExclusiveAsync_v3()
        {

            Task<T> toExecute = null;

            if (this._singleExecute != null)
                toExecute = this._singleExecute;

            if (this._singleExecute == null)
            {
                this._semaphore.Wait();
                this._singleExecute = this._task();
                toExecute = this._singleExecute;
                this._semaphore.Release();
            }

            Task<Task<T>> result = this._lock.LockAsync()
                  .ContinueWith(
                  (lockTask, toExec) =>
                  {
                      Task<T> toX = toExec as Task<T>;

                      if (toX == null)
                          throw new NullReferenceException();

                      Task<T> finished = toX.ContinueWith((taskTask, disposableLock) =>
                      {
                          var dlock = disposableLock as IDisposable;
                          dlock?.Dispose();
                          return taskTask.Result;

                      }, lockTask.Result, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                      return finished;

                  }, toExecute, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return result.Unwrap();
        }

    }
}