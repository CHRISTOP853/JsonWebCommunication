using System.Collections.Generic;

namespace JsonCore.Models
{
    public class Team
    {
        public int TeamNumber { get; set; }
        public string? TeamName { get; set; }
        public int Season { get; set; }
        public List<GameStats>? Games { get; set; } = new ();

        public SeasonRecord Record
        {
            get
            {
                
                int wins = 0, losses = 0, ties = 0;
                foreach (var game in Games)
                {
                    if (game.TeamScore > game.OpponentScore) wins++;
                    else if (game.TeamScore < game.OpponentScore) losses++;
                    else ties++;
                }
                return new SeasonRecord
                {
                    Wins = wins,
                    Losses = losses,
                    Ties = ties
                };
            }
        }
    }
}