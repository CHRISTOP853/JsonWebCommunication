using System;
using System.Threading;
using System.Threading.Tasks;
using JsonCore.Messaging;

namespace JsonGui
{
    public class CommunicationThread : IDisposable
    {
        private readonly ApiRequestQueue _queue;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _worker;
        private static readonly string logFilePath = "commthread_errors.txt";

        public CommunicationThread(ApiRequestQueue queue)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _worker = Task.Run(ProcessQueueAsync);
        }

        private async Task ProcessQueueAsync()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    // ApiRequestQueue returns a Func<Task> (work delegate)
                    var req = _queue.Take(_cts.Token);
                    if (req == null) continue;

                    try
                    {
                        // invoke the delegate and await the work
                        await req().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogErrorToFile(ex, logFilePath);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                // expected when stopping the thread
                ErrorLogger.LogErrorToFile(ex, logFilePath);
            }
            catch (Exception ex)
            {
                // log unexpected exceptions
                ErrorLogger.LogErrorToFile(ex, logFilePath);
            }
        }

        public void Stop()
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _queue.CompleteAdding();
                try { _worker.Wait(2000); } catch { /* ignore */ }
            }
        }

        public void Dispose()
        {
            Stop();
            _cts.Dispose();
        }

    }
}