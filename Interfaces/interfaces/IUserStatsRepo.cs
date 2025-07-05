using Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.interfaces
{
    public interface IUserStatsRepo
    {
        bool IncrementEpisodesWatched(int amount = 1);
        bool IncrementShowsWatched(int amount = 1);
        bool AddTimeWatched(TimeSpan duration);
        bool AddFavouriteEpisode(Episode episode);
        bool RemoveFavouriteEpisode(Episode episode);
        List<Episode> GetFavourites();
        UserStats GetStats();
        bool SaveStats(UserStats stats);
        bool ResetStats();
    }
}
