using Interfaces.Entities;
using Interfaces.interfaces;
using MediaToolkit;
using MediaToolkit.Model;
using Service.Helper;
using System.Text.RegularExpressions;

namespace Service
{
    public class ShowService
    {
        private readonly IShowRepo ShowRepo;

        private readonly ILoggerService logger;
        string[] videoExtensions = { ".mkv", ".mp4", ".avi", ".mov" };

        public ShowService(IShowRepo showRepo, ILoggerService _logger)
        {
            this.ShowRepo = showRepo;
            this.logger = _logger;
        }

        public ResponseBody<Show> GetShowFromName(string showname)
        {
            try
            {
               Show? show = ShowRepo.GetShow(showname);
                if(show != null)
                {
                    return ResponseBody<Show>.Ok(show);
                }
                return ResponseBody<Show>.Fail($"couldnt find show with the name {showname}");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog<Show>(logger, nameof(GetShowFromName), ex, "Unexpected error");
            }
        }
        public ResponseBody<Show> AddShowFromFolder(string folder, string numberingscheme, string showName)
        {
            try
            {
                Show? existing = ShowRepo.GetShow(showName);
                if (existing != null)
                {
                    return ResponseBody<Show>.Fail($"Show '{showName}' already exists.");
                }
                using var engine = new Engine();
                var seasons = new List<Season>();
                

                var regex = new Regex(numberingscheme, RegexOptions.IgnoreCase); // for future episode numbering use

                // Try to create a season from the root folder
                var rootSeason = CreateSeasonFromFolder(folder, 1, showName, engine, regex);
                if (rootSeason != null)
                {
                    seasons.Add(rootSeason);
                }
                else
                {
                    // No root videos, check immediate subfolders for seasons
                    var subfolders = Directory.GetDirectories(folder);
                    int seasonNumber = 1;
                    foreach (var subfolder in subfolders)
                    {
                        var season = CreateSeasonFromFolder(subfolder, seasonNumber, showName, engine, regex);
                        if (season != null)
                        {
                            seasons.Add(season);
                            seasonNumber++;
                        }
                    }

                    if (seasons.Count == 0)
                    {
                        return ResponseBody<Show>.Fail("No video files found in the selected folder or its subfolders.");
                    }
                }

                var allEpisodes = seasons.SelectMany(s => s.Episodes)
                                         .OrderBy(e => e.Season)
                                         .ThenBy(e => e.Number)
                                         .ToList();
                var totalDuration = seasons.Aggregate(TimeSpan.Zero, (sum, s) => sum + s.Duration);

                var newShow = new Show
                {
                    Name = showName,
                    BasePath = folder,
                    Seasons = seasons,
                    IsFinished = false,
                    CurrentEpisodePath = allEpisodes.First().FilePath,
                    Duration = totalDuration,
                    PosterFileName = showName
                };

                bool success = ShowRepo.AddShow(newShow);
                if (!success)
                {
                    logger.LogError($"AddShowFromFolder: Failed to save new show '{newShow.Name}'.");
                    return ResponseBody<Show>.Fail("Failed to save new show.");
                }

                logger.LogInfo($"AddShowFromFolder: Successfully added show '{newShow.Name}'.");
                return ResponseBody<Show>.Ok(newShow);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog<Show>(logger, nameof(AddShowFromFolder), ex, "Unexpected error");
            }
        }
        public ResponseBody<string> GetFirstVideoFile(string folderPath)
        {
            try
            {
                // Check video files directly in the folder
                var rootVideos = GetFilesFromFolder(folderPath);
                if (rootVideos.Count > 0)
                {
                    string fileNameOnly = Path.GetFileName(rootVideos.First());
                    return ResponseBody<string>.Ok(fileNameOnly);
                }

                // If no root videos, check immediate subfolders
                var subfolders = Directory.GetDirectories(folderPath);
                foreach (var subfolder in subfolders)
                {
                    var subfolderVideos = GetFilesFromFolder(subfolder);
                    if (subfolderVideos.Count > 0)
                    {
                        string fileNameOnly = Path.GetFileName(subfolderVideos.First());
                        return ResponseBody<string>.Ok(fileNameOnly);
                    }
                }

                return ResponseBody<string>.Fail("No video files found in the folder or its immediate subfolders.");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog<string>(logger, nameof(GetFirstVideoFile), ex, "Unexpected error");
            }
        }


        public ResponseBody<List<string>> GetAllShowNames()
        {
            try
            {
                var names = ShowRepo.GetAllShowNames();
                return ResponseBody<List<string>>.Ok(names);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog<List<string>>(logger, nameof(GetAllShowNames), ex, "Unexpected error");
            }
        }
        private List<Episode> GetEpisodesFromFolder(string folder, int seasonNumber, string showName, Engine engine, Regex numberRegex, out TimeSpan totalDuration)
        {
            var videoFiles = GetFilesFromFolder(folder);
            var episodes = new List<Episode>();
            totalDuration = TimeSpan.Zero;

            // Temp list to hold filename and extracted episode number
            var episodeFiles = new List<(string file, int? episodeNumber)>();

            foreach (var file in videoFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var match = numberRegex.Match(fileName);
                int? episodeNum = null;

                if (match.Success && int.TryParse(match.Groups[1].Value, out int epNum))
                {
                    episodeNum = epNum;
                }

                episodeFiles.Add((file, episodeNum));
            }

            // Sort files by extracted episode number; if no number found, keep original order
            var orderedFiles = episodeFiles.OrderBy(ef => ef.episodeNumber ?? int.MaxValue).ToList();

            // Now create Episode objects with correct numbering
            for (int i = 0; i < orderedFiles.Count; i++)
            {
                var file = orderedFiles[i].file;
                var inputFile = new MediaFile { Filename = file };
                engine.GetMetadata(inputFile);

                var duration = inputFile.Metadata?.Duration ?? TimeSpan.Zero;

                episodes.Add(new Episode
                {
                    Number = i + 1,  // episode number assigned by order
                    Title = Path.GetFileNameWithoutExtension(file),
                    FilePath = file,
                    WhenWatched = null,
                    ShowName = showName,
                    Season = seasonNumber,
                    Duration = duration
                });

                totalDuration += duration;
            }

            return episodes;
        }
        private Season? CreateSeasonFromFolder(string folder, int seasonNumber, string showName, Engine engine, Regex numberRegex)
        {
            var episodes = GetEpisodesFromFolder(folder, seasonNumber, showName, engine, numberRegex, out TimeSpan totalDuration);
            if (episodes.Count == 0) return null;

            return new Season
            {
                Number = seasonNumber,
                Episodes = episodes,
                Path = folder,
                Duration = totalDuration
            };
        }
        private List<string> GetFilesFromFolder(string path)
        {
            return Directory
                .EnumerateFiles(path)
                .Where(f => videoExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .ToList();
        }
        public ResponseBody<List<string>> GetAllShowNamesFromApp()
        {
            var names =  ShowRepo.GetShowNamesInApp();
            return ResponseBody<List<string>>.Ok(names);
        }
    }
}
