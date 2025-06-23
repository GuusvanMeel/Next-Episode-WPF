namespace Interfaces.Entities
{
    public class Episode
    {
        public required int Season { get; set; }
        public required int Number { get; set; }

        public required string FilePath { get; set; }
        public required string Title { get; set; }
        public required string ShowName { get; set; }
        public TimeSpan Duration { get; set; }

        public DateTime? WhenWatched { get; set; }
    }
}
