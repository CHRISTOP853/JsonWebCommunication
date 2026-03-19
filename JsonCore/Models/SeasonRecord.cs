namespace JsonCore.Models
{
    public class SeasonRecord
    {
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
        public override string ToString()
        {
            return $"{Wins}-{Losses}" + (Ties > 0 ? $"-{Ties}" : "");
        }
    }
}