using System;
using System.Threading;
using System.Threading.Tasks;

namespace TimeClock.Helpers
{
    public static class RetryHelper
    {
        public static void RetryOnException(int times, TimeSpan delay, Action operation, CancellationToken cancellationToken = default)
        {
            var attempts = 0;
            do
            {
                try
                {
                    attempts++;
                    operation();
                    break;
                }
                catch (Exception) when (attempts < times)
                {
                    Task.Delay(delay, cancellationToken).Wait(cancellationToken);
                }
            } while (true);
        }

        public static async Task RetryOnExceptionAsync(int times, TimeSpan delay, Func<Task> operation, CancellationToken cancellationToken = default)
        {
            await RetryOnExceptionAsync<Exception>(times, delay, operation, cancellationToken);
        }

        public static async Task RetryOnExceptionAsync<TException>(int times, TimeSpan delay, Func<Task> operation, CancellationToken cancellationToken = default) where TException : Exception
        {
            if (times <= 0)
                throw new ArgumentOutOfRangeException(nameof(times));

            var attempts = 0;
            do
            {
                try
                {
                    attempts++;
                    await operation();
                    break;
                }
                catch (TException) when (attempts < times)
                {
                    await Task.Delay(delay, cancellationToken);
                }
            } while (true);
        }
    }
}

/** 
TODO - Polly Library: Consider using the Polly library, which provides a more robust and flexible way to handle retries and other resilience strategies.

*/