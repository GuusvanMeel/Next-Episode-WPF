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
        public Show()
        {

        }
    }
}
