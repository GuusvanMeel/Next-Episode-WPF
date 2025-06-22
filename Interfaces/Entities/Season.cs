namespace Interfaces.Entities
{
    public class Season
    {
        public required string Path { get; set; }
        public int Number { get; set; }
        public List<Episode> Episodes { get; set; } = new();
    }
}
