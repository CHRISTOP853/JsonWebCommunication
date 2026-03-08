using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net.Http;
using System.Threading.Tasks;

namespace JsonCore.Services;

public class JsonTextService
{
 private static readonly HttpClient _http = new HttpClient();
    public JsonNode Parse(string jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText ))
            throw new ArgumentException("No JSON to parse.");
                return JsonNode.Parse(jsonText)?? 
                throw new JsonException(" Parsed JSON root is null or invalid.");
    }

    public string PrettyPrint(string json)
    {
        var node = Parse(json);
        return node.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public async Task<string> LoadFromUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL is missing.");

        return await _http.GetStringAsync(url);
    }
}
