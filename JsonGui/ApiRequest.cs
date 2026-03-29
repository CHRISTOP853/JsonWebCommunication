using System;
using System.Threading.Tasks;

namespace JsonGui
{
    public interface IApiRequest // Marker interface for API requests
    {
        Task ExecuteAsync();
    }

    public class ApiRequest<T> : IApiRequest // Represents an API request that returns a result of type T
    {
        public Func<Task<T>> Work { get; }
        public TaskCompletionSource<T> Tcs { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ApiRequest(Func<Task<T>> work) => Work = work ?? throw new ArgumentNullException(nameof(work));

        public async Task ExecuteAsync()
        {
            try
            {
                var result = await Work().ConfigureAwait(false);
                Tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                Tcs.SetException(ex);
            }
        }
    }
}