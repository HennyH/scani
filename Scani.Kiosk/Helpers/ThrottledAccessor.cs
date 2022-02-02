using Microsoft.VisualStudio.Threading;

namespace Scani.Kiosk.Helpers
{
    public class ThrottledAccessor<T>
    {
        private readonly ILogger<ThrottledAccessor<T>> _logger;
        private readonly T _resource;
        private readonly int _limit;
        private readonly TimeSpan _limitPeriod;
        private int _periodUsages;
        private DateTime _periodStart;

        public ThrottledAccessor(ILogger<ThrottledAccessor<T>> logger, T resource, int limit, TimeSpan limitPeriod)
        {
            this._logger = logger;
            this._resource = resource;
            this._limit = limit;
            this._limitPeriod = limitPeriod;
            this._periodUsages = 0;
            this._periodStart = DateTime.UtcNow;
        }

        public virtual async Task AccessAsync(Func<T, Task> action, TimeSpan? interval = null)
        {
            ArgumentNullException.ThrowIfNull(action);

            await AccessAsync(async (r) =>
            {
                await action(r);
                return Task.CompletedTask;
            }, interval);
        }

        public virtual async Task<TResult> AccessAsync<TResult>(Func<T, Task<TResult>> action, TimeSpan? interval = null)
        {
            ArgumentNullException.ThrowIfNull(action);

            var intervalMilliseconds = (int)interval.GetValueOrDefault(TimeSpan.FromMilliseconds(200)).TotalMilliseconds;

            while (true)
            {
                var usagePeriodDuration = DateTime.UtcNow - _periodStart;

                if (usagePeriodDuration > _limitPeriod)
                {
                    _logger.LogInformation("Reset throttled accessor period at {}", _periodStart);
                    _periodStart = DateTime.UtcNow;
                    Interlocked.Exchange(ref _periodUsages, 0);
                }

                if (_periodUsages < _limit)
                {
                    var usages = Interlocked.Increment(ref _periodUsages);
                    _logger.LogInformation("Throttled accessor allow access resulting in period usage total of {}", usages);
                    return await action(_resource);
                }
                else
                {
                    _logger.LogWarning("Throttled accessor period reached limit usage of {} every {}, duration of current usage period is {}", _limit, _limitPeriod, usagePeriodDuration);
                    await Task.Delay(intervalMilliseconds);
                }
            }
        }

        public void Dispose()
        {
            if (_resource is IDisposable disposable) disposable.Dispose();
        }
    }
}
