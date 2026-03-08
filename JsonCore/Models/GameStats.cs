using System.Text.Json.Serialization;

namespace JsonCore.Models
{
    public class GameStats
    {
        [JsonPropertyName("gameId")]
        public string? GameId { get; set; }

        [JsonPropertyName("gameDate")]
        public string? GameDate { get; set; }

        [JsonPropertyName("opponent")]
        public string? Opponent { get; set; }

        [JsonPropertyName("homeAway")]
        public string? HomeAway { get; set; }

        [JsonPropertyName("teamScore")]
        public int TeamScore { get; set; }

        [JsonPropertyName("opponentScore")]
        public int OpponentScore { get; set; }

        [JsonPropertyName("passYards")]
        public int? PassYards { get; set; }

        [JsonPropertyName("rushYards")]
        public int? RushYards { get; set; }

        [JsonPropertyName("totalYards")]
        public int? TotalYards { get; set; }
    }
}