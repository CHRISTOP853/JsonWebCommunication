using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace JsonCore.Messaging
{
    public class ApiRequestQueue
    {
        private readonly ConcurrentQueue<Func<Task>> _queue = new();
        private bool _processing;

        public void Enqueue(Func<Task> request)
        {
            _queue.Enqueue(request);
            ProcessQueue();
        }

        private async void ProcessQueue()
        {
            if (_processing) return;

            _processing = true;

            try
            {
                while (_queue.TryDequeue(out var request))
                {
                    try
                    {
                        await request();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    await Task.Delay(250);
                }
            }
            finally
            {
                _processing = false;
            }
        }
    }
}
