using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor.Services
{
    public static class RetryStrategy
    {
        public static async Task<T> RetryAsync<T>(Func<Task<T>> func, int maxRetryCount, int initialDelayMs, CancellationToken ct, Func<Exception, bool> shouldRetry)
        {
            int attempt = 0;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var result = await func.Invoke();

                    return result;
                }
                catch (Exception ex)
                {
                    attempt++;
                    if (attempt > maxRetryCount || !shouldRetry.Invoke(ex))
                    {
                        throw;
                    }

                    int rawDelay = initialDelayMs * (int)Math.Pow(2, attempt - 1);
                    int delay = Math.Min(rawDelay, 1000);
                    LogService.Warn($"执行失败，第{attempt}次重试，等待{delay}ms,{ex}");
                    await Task.Delay(delay, ct);
                }
            }
        }
        public static async Task RetryAsync(Func<Task> func, int maxRetryCount,int initialDelayMs,CancellationToken ct,Func<Exception, bool> shouldRetry)
        {
            await RetryAsync<object>(
                func: async () =>
                {
                    await func();
                    return null;
                },
                maxRetryCount: maxRetryCount,
                initialDelayMs: initialDelayMs,
                ct: ct,
                shouldRetry: shouldRetry
            );
        }

        public static bool IsModbusTransientException(Exception ex)
        {
            return ex is SocketException se && se.SocketErrorCode == SocketError.TimedOut;
        }
    }
}
