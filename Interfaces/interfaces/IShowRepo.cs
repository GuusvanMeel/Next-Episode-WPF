using Interfaces.Entities;

namespace Interfaces.interfaces
{
    public interface IShowRepo
    {
        public List<Show> LoadAllShows();
        public Show? GetShow(string showname);
        public List<string> GetAllShowNames();
        public bool AddShow(Show show);
        public bool DeleteShow(string showname);
        public bool AddSeason(Season season, string showname);
        public bool DeleteSeason(int number, string showname);
        public bool SaveShow(Show show);
        public List<string> GetShowNamesInApp();
    }
}
