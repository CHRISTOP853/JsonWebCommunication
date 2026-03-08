using System;
using System.Threading.Tasks;   
using JsonCore.Api;
using JsonCore.Models;  
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;

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
            // TODO: Implement team statistics retrieval
            string json = await _api.GetTeamSeasonAsync(teamNumber, season);

            JsonNode node = JsonNode.Parse(json) ?? 
            throw new JsonException("response to JSON is invalid.");

            JsonArray? gamesArray =  node["data"]?["games"]?.AsArray();

            if (gamesArray is null)                
            throw new JsonException("Could not find games array in Snoozle response.Update JSON path in TeamStatsService.");

            var games = new List<GameStats>();
            foreach (var item in gamesArray)
            {
                if(item is null)
                
                 continue;

                 var gs = item.Deserialize<GameStats>();
                 if(gs is not null)
                    {
                        games.Add(gs);
                        
                    }
            }
                    var team = new Team
                    {
                        TeamNumber = teamNumber,
                        Season = season,
                        Games = games
                    };

                    return team;
               
            }
       
        }
    }


