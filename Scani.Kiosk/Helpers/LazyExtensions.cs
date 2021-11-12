﻿using System.Runtime.CompilerServices;

namespace Scani.Kiosk.Extensions
{
    public static class LazyExtensions
    {
        public static TaskAwaiter<T> GetAwaiter<T>(this Lazy<Task<T>> asyncTask)
        {
            ArgumentNullException.ThrowIfNull(asyncTask);
            return asyncTask.Value.GetAwaiter();
        }
    }
}
