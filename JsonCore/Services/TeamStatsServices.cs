using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using JsonCore.Api;
using JsonCore.Models;

namespace JsonCore.Services
{
    public class TeamStatsService
    {
        private readonly SnoozleApiClient _api;

        public TeamStatsService(SnoozleApiClient api)
        {
            _api = api;
        }

        public async Task<Team> GetTeamSeasonAsync(int teamNumber, int season = 2020)
        {
            string json = await _api.GetTeamSeasonAsync(teamNumber, season);

            JsonNode? node = JsonNode.Parse(json);
            if (node == null)
                throw new JsonException("Response JSON is invalid.");

            JsonArray? matchupArray =
                node["matchUpStats"] as JsonArray
                ?? node["matchupStats"] as JsonArray
                ?? node["matchupstats"] as JsonArray;

            if (matchupArray == null)
                throw new JsonException("Could not locate matchUpStats array in Snoozle response.");

            var games = new List<GameStats>();

            foreach (JsonNode? item in matchupArray)
            {
                if (item == null)
                    continue;

                JsonNode? visStats =
                    item["visStats"]
                    ?? item["visstats"];

                JsonNode? homeStats =
                    item["homeStats"]
                    ?? item["homestats"];

                if (visStats == null || homeStats == null)
                    continue;

                int? visTeamCode =
                    visStats["teamCode"]?.GetValue<int>()
                    ?? visStats["teamcode"]?.GetValue<int>();

                int? homeTeamCode =
                    homeStats["teamCode"]?.GetValue<int>()
                    ?? homeStats["teamcode"]?.GetValue<int>();

                JsonNode? chosenStats = null;

                if (visTeamCode == teamNumber)
                    chosenStats = visStats;
                else if (homeTeamCode == teamNumber)
                    chosenStats = homeStats;

                if (chosenStats == null)
                    continue;

                GameStats? gs = chosenStats.Deserialize<GameStats>(
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (gs != null)
                    games.Add(gs);
            }

            return new Team
            {
                TeamNumber = teamNumber,
                Season = season,
                Games = games
            };
        }
    }
}
