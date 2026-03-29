using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace JsonCore.Messaging // Thread safe producer/consumer queue for API requests.
{

    // Now, consumer can use Take(CancellationToken) / TryTake() to implement an explicit communication thread.

    public class ApiRequestQueue : IDisposable
    {
        private readonly BlockingCollection<Func<Task>> _queue = new(new ConcurrentQueue<Func<Task>>());
        private readonly object _locker = new();
        private Task? _processor;
        private CancellationTokenSource? _processorCts;
        private bool _disposed;

        public void Enqueue(Func<Task> request) // enqueue a request
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (_disposed) throw new ObjectDisposedException(nameof(ApiRequestQueue));

            _queue.Add(request);
            EnsureProcessorRunning();
        }


        public Func<Task> Take(CancellationToken ct) => _queue.Take(ct); //Blocking take for an external consumer/communication thread.

        public bool TryTake(out Func<Task>? request) => _queue.TryTake(out request); // Try to take without blocking.

        /// <summary>
        /// Signal that no more items will be enqueued.
        /// </summary>
        public void CompleteAdding() => _queue.CompleteAdding();

        private void EnsureProcessorRunning()
        {
            lock (_locker)
            {
                if (_processor != null && !_processor.IsCompleted) return;

                _processorCts = new CancellationTokenSource();
                _processor = Task.Run(() => ProcessQueueAsync(_processorCts.Token));
            }
        }

        // dequeues and executes requests sequentially.
        private async Task ProcessQueueAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && !_queue.IsCompleted)
                {
                    Func<Task> request;
                    try
                    {
                        request = _queue.Take(ct);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (InvalidOperationException) { break; } // collection completed

                    try
                    {
                        await request().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    try
                    {
                        await Task.Delay(250, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { break; }
                }
            }
            finally
            {
                // cleanup if needed
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { _processorCts?.Cancel(); } catch { }

            _queue.CompleteAdding();

            try
            {
                _processor?.Wait(1000);
            }
            catch { /* ignore */ }

            _processorCts?.Dispose();
            _queue.Dispose();
        }
    }
}
