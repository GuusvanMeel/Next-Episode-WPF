namespace Interfaces.Entities
{
    public class Show
    {
        public required string Name { get; set; }
        public required string BasePath { get; set; }
        public List<Season> Seasons { get; set; } = new();
        public string? CurrentEpisodePath { get; set; }
        public bool IsFinished { get; set; }
        public TimeSpan Duration { get; set; }
        public required string PosterFileName { get; set; }
        public TimeSpan TimeWatched
        {
            get
            {
                return Seasons
                    .SelectMany(s => s.Episodes)
                    .Where(e => e.WhenWatched != null)
                    .Aggregate(TimeSpan.Zero, (sum, ep) => sum + ep.Duration);
            }
        }

        public Show()
        {

        }
    }
}
