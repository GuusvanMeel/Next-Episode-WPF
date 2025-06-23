using Interfaces.Entities;
using Interfaces.interfaces;

namespace Service
{
    public class ShowService
    {
        private IShowRepo ShowRepo;
        public ShowService(IShowRepo _repo)
        {
            this.ShowRepo = _repo;
        }
        //addshow you need to check if a show with that name already exists before adding, overwriting it.
        public ResponseBody<Show> AddShowFromFolder()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select folder for the new show"
            };

            if (dialog.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                return ResponseBody<Show>.Fail("No folder selected.");
            }

            string folderPath = dialog.SelectedPath;
            string showName = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar));

            if (ShowRepo.GetAllShowNames().Contains(showName))
            {
                return ResponseBody<Show>.Fail($"A show named '{showName}' already exists.");
            }

            // Scan video files (you can add more extensions if needed)
            var videoExtensions = new[] { ".mkv", ".mp4", ".avi", ".mov" };
            var videoFiles = Directory
                .EnumerateFiles(folderPath)
                .Where(f => videoExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .OrderBy(f => f) // order by filename, customize if you want
                .ToList();

            if (videoFiles.Count == 0)
            {
                return ResponseBody<Show>.Fail("No video files found in the selected folder.");
            }

            // Create episodes from files
            var episodes = videoFiles.Select((file, index) => new Episode
            {
                Number = index + 1,
                Title = Path.GetFileNameWithoutExtension(file),
                FilePath = file,
                WhenWatched = null,
                ShowName = showName
            }).ToList();

            // Create a single season with these episodes
            var season = new Season
            {
                Number = 1,
                Episodes = episodes,
                Path = folderPath
            };

            var newShow = new Show
            {
                Name = showName,
                BasePath = folderPath,
                Seasons = new List<Season> { season },
                IsFinished = false,
                CurrentEpisodePath = episodes.First().FilePath // set first episode as current
            };

            bool success = ShowRepo.AddShow(newShow);
            if (!success)
            {
                return ResponseBody<Show>.Fail("Failed to save new show.");
            }

            return ResponseBody<Show>.Ok(newShow);
        }

        public ResponseBody<List<string>> GetAllShowNames()
        {
            return ResponseBody<List<string>>.Ok(ShowRepo.GetAllShowNames());
        }

    }
}
