namespace Interfaces.Entities
{
    public class UserStats
    {
        public TimeSpan TotalTimeWatched { get; set; }

        public int EpsWatched { get; set; }

        public int ShowsCompleted { get; set; }

        public List<Episode>? Favourites { get; set; }
    }
}
