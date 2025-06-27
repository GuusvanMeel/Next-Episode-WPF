using Interfaces.Entities;
using Interfaces.interfaces;
using Service.Helper;
using static Interfaces.Entities.ActivityLog;

namespace Service
{
    public class ActivityService(IActivityLogRepo _repo, ILoggerService _logger)
    {
        IActivityLogRepo ActivityRepo = _repo;

        ILoggerService logger = _logger;
        public ResponseBody EditedShow(string showtitle)
        {
            try
            {
                var log = new ActivityLog
                {
                    Type = ActivityType.EditedShow,
                    ShowTitle = showtitle,
                    When = DateTime.Now
                };

                var result = ActivityRepo.Log(log);
                return ResponseHelper.Check(result);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog(logger, nameof(LogShowFinished), ex, "Failed to log activity");
            }
        }
        public ResponseBody AddedShow(string showtitle)
        {
            try
            {
                var log = new ActivityLog
                {
                    Type = ActivityType.AddedShow,
                    ShowTitle = showtitle,
                    When = DateTime.Now
                };

                var result = ActivityRepo.Log(log);
                return ResponseHelper.Check(result);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog(logger, nameof(LogShowFinished), ex, "Failed to log activity");
            }
        }
        public ResponseBody ResetShowProgress(string showtitle)
        {
            try
            {
                var log = new ActivityLog
                {
                    Type = ActivityType.ResetProgress,
                    ShowTitle = showtitle,
                    When = DateTime.Now
                };

                var result = ActivityRepo.Log(log);
                return ResponseHelper.Check(result);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog(logger, nameof(LogShowFinished), ex, "Failed to log activity");
            }
        }
        public ResponseBody LogShowFinished(string showtitle)
        {
            try
            {
                var log = new ActivityLog
                {
                    Type = ActivityType.ShowFinished,
                    ShowTitle = showtitle,
                    When = DateTime.Now
                };

                var result = ActivityRepo.Log(log);
                return ResponseHelper.Check(result);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog(logger, nameof(LogShowFinished), ex, "Failed to log activity");
            }
        }

        public ResponseBody<ActivityLog> LogEpisodeWatched(string showTitle, int season, int episode, DateTime? when = null)
        {
            try
            {
                var log = new ActivityLog
                {
                    Type = ActivityLog.ActivityType.WatchedEpisode,
                    ShowTitle = showTitle,
                    Season = season,
                    Episode = episode,
                    When = when ?? DateTime.Now
                };

                var result = ActivityRepo.Log(log);
                if (!result.Success)
                {
                    return ResponseBody<ActivityLog>.Fail(result.Message ?? "Failed to save activity log.");
                }

                return ResponseBody<ActivityLog>.Ok(log);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog<ActivityLog>(logger, nameof(LogEpisodeWatched), ex, "Failed to log activity");
            }
        }
        public ResponseBody<List<ActivityLog>> GetActivity()
        {
            var activities = ActivityRepo.GetAll();
            return ResponseHelper.Check(activities);
        }
        public ResponseBody<ActivityLog> GetMostRecent()
        {
            var activity = ActivityRepo.GetLast();
            return ResponseHelper.Check(activity);
        }


    }
}
