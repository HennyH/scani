namespace Scani.Kiosk.Helpers
{
    public static class EventHelpers
    {
        public static Task InvokeAllAsync<T>(this Func<T, Task>? @delegate, T arg)
        {
            var tasks = @delegate
                ?.GetInvocationList()
                ?.Cast<Func<T, Task>>()
                ?.Select(f => f(arg));

            if (tasks == null)
            {
                return Task.CompletedTask;
            }

            return Task.WhenAll(tasks);
        }

        public static Task InvokeAllAsync(this Func<Task>? @delegate)
        {
            var tasks = @delegate
                ?.GetInvocationList()
                ?.Cast<Func<Task>>()
                ?.Select(f => f());

            if (tasks == null)
            {
                return Task.CompletedTask;
            }

            return Task.WhenAll(tasks);
        }
    }
}
