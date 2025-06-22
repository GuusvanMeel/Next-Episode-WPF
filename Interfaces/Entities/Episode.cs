namespace Interfaces.Entities
{
    public class Episode
    {
        public int Season { get; set; }
        public int Number { get; set; }

        public required string FilePath { get; set; }
        public required string Title { get; set; }
        public required string ShowName { get; set; }

        public DateTime? WhenWatched { get; set; }
    }
}
