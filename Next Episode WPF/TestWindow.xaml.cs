using Interfaces.Entities;
using Interfaces.interfaces;
using Service;
using System.Windows;

namespace Next_Episode_WPF
{
    public partial class TestWindow : Window
    {
        private readonly IShowRepo showRepo;
        private readonly EpisodeService episodeService;
        private readonly PlayerService playerService;
        private readonly ShowService _showService;

        private Show? selectedShow;

        public TestWindow(IShowRepo showRepo, EpisodeService episodeService, PlayerService playerService, ShowService showService)
        {
            InitializeComponent();

            this.showRepo = showRepo;
            this.episodeService = episodeService;
            this.playerService = playerService;
            this._showService = showService;

            LoadShows();
        }

        private void AddShowFromFolder_Click(object sender, RoutedEventArgs e)
        {
            var result = _showService.AddShowFromFolder();
            if (result.Success && result.Data != null)
            {
                LogOutput.Text = $"Show '{result.Data.Name}' added successfully.";
                LoadShows(); // refresh show list
            }
            else
            {
                LogOutput.Text = result.Message ?? "Failed to add show.";
            }
        }

        private void LoadShows()
        {
            var names = showRepo.GetAllShowNames();
            ShowSelector.ItemsSource = names;
        }

        private void ShowSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ShowSelector.SelectedItem is string name)
            {
                selectedShow = showRepo.GetShow(name);
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (selectedShow == null)
            {
                CurrentEpisodeBlock.Text = "No show selected.";
                ProgressBar.Value = 0;
                return;
            }

            var currentEp = episodeService.GetCurrentEpisode(selectedShow);
            if (currentEp.Success && currentEp.Data != null)
            {
                CurrentEpisodeBlock.Text = $"Current Episode: {currentEp.Data.Title}";
            }
            else
            {
                CurrentEpisodeBlock.Text = "No current episode set.";
            }

            var progress = episodeService.GetWatchProgress(selectedShow);
            ProgressBar.Value = progress.Success ? progress.Data : 0;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShow == null)
            {
                LogOutput.Text = "No show selected.";
                return;
            }

            var currentEp = episodeService.GetCurrentEpisode(selectedShow);
            if (!currentEp.Success || currentEp.Data == null)
            {
                LogOutput.Text = currentEp.Message ?? "Current episode not found.";
                return;
            }

            var result = playerService.StartVideo(currentEp.Data.FilePath);
            LogOutput.Text = result.Success ? "Player launched." : result.Message;
        }
        private void SetVideoPlayerPath_Click(object sender, RoutedEventArgs e)
        {
            // You can call your ShowService or PlayerService method to set the path here.
            // For example, open a FileDialog to select the player executable,
            // then call the service to save the path.

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Video Player Executable",
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*"
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string playerPath = openFileDialog.FileName;

                var response = playerService.SetVideoPlayer(playerPath); // Or playerService if appropriate

                LogOutput.Text = response.Success ? $"Video player set to:\n{playerPath}" : response.Message;
            }
        }


        private void MarkWatched_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShow == null)
            {
                LogOutput.Text = "No show selected.";
                return;
            }

            var result = episodeService.LogEpisodeWatched(selectedShow);
            if (result.Success && result.Data != null)
            {
                selectedShow = result.Data;
                LogOutput.Text = "Episode marked as watched.";
            }
            else
            {
                LogOutput.Text = result.Message ?? "Failed to mark episode as watched.";
            }

            UpdateUI();
        }
    }
}
