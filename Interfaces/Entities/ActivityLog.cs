namespace Interfaces.Entities
{
    public class ActivityLog
    {
        public DateTime When { get; set; } = DateTime.Now;

        public string? ShowTitle { get; set; }

        public int? Season { get; set; }

        public int? Episode { get; set; }

        public string? EpisodeTitle { get; set; }

        public ActivityType Type { get; set; }

        public enum ActivityType
        {
            WatchedEpisode,
            ShowFinished,
            ResetProgress,
            AddedShow,
            EditedShow,
        }
    }
}
