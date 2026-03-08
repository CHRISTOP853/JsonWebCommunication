using System.Collections.Concurrent;
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

        while (_queue.TryDequeue(out var request))
        {
            await request();
        }

        _processing = false;
    }
}
}