using Interfaces.Entities;
using Interfaces.interfaces;
using Service.Helper;

namespace Service
{
    public class UserService(ILoggerService _logger, IUserStatsRepo _statsrepo)
    {
        ILoggerService logger = _logger;

        IUserStatsRepo StatsRepo = _statsrepo;

        public ResponseBody UpdateEpisodesWatched(int amount = 1)
        {
            try
            {
                bool result = StatsRepo.IncrementEpisodesWatched(amount);
                if (result)
                {
                    return ResponseBody.Ok();
                }
                return ResponseBody.Fail("Failed to update episodes watched");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog(logger, nameof(UpdateEpisodesWatched), ex, "failed to update episodes watched");
            }
        }

        public ResponseBody UpdateShowsWatched(int amount = 1)
        {
            try
            {
                bool result = StatsRepo.IncrementShowsWatched(amount);
                if (result)
                {
                    return ResponseBody.Ok();
                }
                return ResponseBody.Fail("Failed to update Shows watched");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog(logger, nameof(UpdateShowsWatched), ex, "failed to update Shows watched");
            }
        }

        public ResponseBody IncreaseTimeWatched(TimeSpan duration)
        {
            try
            {
                bool result = StatsRepo.AddTimeWatched(duration);
                if (result)
                {
                    return ResponseBody.Ok();
                }
                return ResponseBody.Fail("Failed to update time watched");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog(logger, nameof(IncreaseTimeWatched), ex, "failed to update time watched");
            }
        }

        public ResponseBody ToggleFavouriteEpisodes(Episode ep)
        {
            try
            {
                bool result;

                List<Episode> episodes = StatsRepo.GetFavourites();
                if (episodes.Any(e => e.FilePath.Equals(ep.FilePath, StringComparison.OrdinalIgnoreCase)))
                {
                    result = StatsRepo.RemoveFavouriteEpisode(ep);
                }
                else
                {
                    result = StatsRepo.AddFavouriteEpisode(ep);
                }
                if (result)
                {
                    return ResponseBody.Ok();
                }
                return ResponseBody.Fail("Failed to toggle favourite episode");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog(logger, nameof(ToggleFavouriteEpisodes), ex, "failed to toggle favourite episode");
            }
        }

        public ResponseBody<UserStats> GetUserStats()
        {
            try
            {
                UserStats result = StatsRepo.GetStats();
                if(result != null)
                {
                    return ResponseBody<UserStats>.Ok(result);
                }
                return ResponseBody<UserStats>.Fail("Failed to get the users statistics");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog<UserStats>(logger, nameof(GetUserStats), ex, "failed to Get user stats");
            }
        }
    }
}
