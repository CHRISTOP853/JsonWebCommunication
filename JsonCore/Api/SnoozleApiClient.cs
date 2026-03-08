using System;
using System.Data;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JsonCore.Services;

namespace JsonCore.Api
{
    public class SnoozleApiClient
    {
        private static readonly HttpClient _http = new HttpClient();
        private const string BaseUrl = "https://api.snoozle.net/v1";

        public async Task<string> GetTeamSeasonAsync(int teamNumber, int season = 2020)
        {
            if (teamNumber < 1 ||teamNumber > 32)
                throw new ArgumentOutOfRangeException(nameof(teamNumber),"Team number must be in the range of 1-32.");

                if (season < 1920 || season > 2100)
                    throw new ArgumentOutOfRangeException(nameof(season),"Season invalid.");

                     var url = $"{BaseUrl}/TeamStats?season={season}&teamNumber={teamNumber}";


                    using var response = await _http.GetAsync(url);

                    if(!
                    response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
            
                        throw new HttpRequestException($"Failed to retrieve team stats. Status code: {response.StatusCode}");

      
            }
            
            return await response.Content.ReadAsStringAsync();
           
        }
    }
}