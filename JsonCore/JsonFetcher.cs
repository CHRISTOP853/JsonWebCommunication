using System.Net.Http;
using System.Threading.Tasks;

namespace JsonCore
{
public class JsonFetcher
{
    private static readonly HttpClient _http = new HttpClient();
    public async Task<string> FetchJsonAsync(string url)
    {
        return await _http.GetStringAsync(url); 
        
    }
}
}

