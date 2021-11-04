using Microsoft.VisualStudio.Threading;
using System.Runtime.CompilerServices;

namespace Scani.Kiosk.Helpers
{
    public class ThrottledAccessor<T>
    {
        private readonly T _resource;
        private readonly int _limit;
        private readonly TimeSpan _limitPeriod;
        private int _periodUsages;
        private DateTime _periodStart;

        public ThrottledAccessor(T resource, int limit, TimeSpan limitPeriod)
        {
            this._resource = resource;
            this._limit = limit;
            this._limitPeriod = limitPeriod;
            this._periodUsages = 0;
            this._periodStart = DateTime.UtcNow;
        }

        public virtual async Task AccessAsync(Func<T, Task> action, TimeSpan? interval = null)
        {
            await AccessAsync(async (r) =>
            {
                await action(r);
                return Task.CompletedTask;
            }, interval);
        }

        public virtual async Task<R> AccessAsync<R>(Func<T, Task<R>> action, TimeSpan? interval = null)
        {
            var intervalMilliseconds = (int)interval.GetValueOrDefault(TimeSpan.FromMilliseconds(200)).TotalMilliseconds;

            while (true)
            {
                var usagePeriodDuration = _periodStart - DateTime.UtcNow;

                if (usagePeriodDuration > _limitPeriod)
                {
                    //_logger.LogTrace("Reset throttled accessor period at {}", _periodStart);
                    _periodStart = DateTime.UtcNow;
                    Interlocked.Exchange(ref _periodUsages, 0);
                }

                if (_periodUsages < _limit)
                {
                    var usages = Interlocked.Increment(ref _periodUsages);
                    //_logger.LogTrace("Throttled accessor allow access resulting in period usage total of {}", usages);
                    return await action(_resource);
                }
                else
                {
                    //_logger.LogTrace("Throttled accessor period reached limit usage of {} every {}, duration of current usage period is {}", _limit, _limitPeriod, usagePeriodDuration);
                    await Task.Delay(intervalMilliseconds);
                }
            }
        }

        public void Dispose()
        {
            if (_resource is IDisposable disposable) disposable.Dispose();
        }
    }

    public class LazyAsyncThrottledAccessor<T> : IDisposable
    {
        private readonly ThrottledAccessor<AsyncLazy<T>> _innerAccessor;

        public LazyAsyncThrottledAccessor(Func<Task<T>> initalizer, int limit, TimeSpan limitPeriod)
        {
            this._innerAccessor = new ThrottledAccessor<AsyncLazy<T>>(new AsyncLazy<T>(initalizer), limit, limitPeriod);
        }

        public async Task AccessAsync(Func<T, Task> action, TimeSpan? interval = null)
            => await _innerAccessor.AccessAsync(async lazyResource => await action(await lazyResource.GetValueAsync()), interval);

        public async Task<R> AccessAsync<R>(Func<T, Task<R>> action, TimeSpan? interval = null)
            => await _innerAccessor.AccessAsync(async lazyResource => await action(await lazyResource.GetValueAsync()), interval);

        public void Dispose()
        {
            _innerAccessor?.Dispose();
        }
    }
}
