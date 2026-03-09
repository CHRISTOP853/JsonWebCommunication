using System.Collections.Generic;

namespace JsonCore.Models
{
    public class Team
    {
        public int TeamNumber { get; set; }
        public string? TeamName { get; set; }
        public int Season { get; set; }
        public List<GameStats>? Games { get; set; } = new ();

        public SeasonRecord Record => new SeasonRecord
{
    Wins = 0,
    Losses = 0,
    Ties = 0
};
    }
}