using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace JsonCore.Api
{
    public class SnoozleApiClient
    {
        private static readonly HttpClient _http = new HttpClient();

        private const string BaseUrl =
            "https://sports.snoozle.net/search/nfl/searchHandler?fileType=inline&statType=teamStats";

        public async Task<string> GetTeamSeasonAsync(int teamNumber, int season = 2020)
        {
            if (teamNumber < 1 || teamNumber > 32)
                throw new ArgumentOutOfRangeException(nameof(teamNumber), "Team number must be in the range 1-32.");

            if (season < 1920 || season > 2100)
                throw new ArgumentOutOfRangeException(nameof(season), "Season invalid.");

            var url = $"{BaseUrl}&season={season}&teamName={teamNumber}";

            using var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"URL: {url}\n\nStatus code: {response.StatusCode}\n\nBody: {body}");
            }

            return body;
        }
    }
}