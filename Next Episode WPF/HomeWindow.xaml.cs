using Interfaces.Entities;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Next_Episode_WPF
{
    /// <summary>
    /// Interaction logic for HomeWindow.xaml
    /// </summary>
    public partial class HomeWindow : Window
    {
        private readonly EpisodeService episodeService;

        private readonly PlayerService playerService;

        private readonly ShowService showService;

        private Show? CurrentShow { get; set; }

        public HomeWindow(EpisodeService epservice, PlayerService playservice, ShowService showservice)
        {
            InitializeComponent();
            this.episodeService = epservice;
            this.playerService = playservice;
            this.showService = showservice;
            RefreshUI();
        }


  
        private void WatchNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (GetCurrentEpisode() is not Episode episode)
            {
                LogOutput.Text = "No episode available to play.";
                return;
            }

            var startvideo = playerService.StartVideo(episode.FilePath);
            if (HandleFailure(startvideo)) return;

            var logEpisode = episodeService.LogEpisodeWatched(CurrentShow!);
            if (HandleFailure(logEpisode)) return;

            UpdateNExtEpisodeInfo();
        }
        private void MarkWatchedButton_Click(object sender, RoutedEventArgs e)
        {
            if (GetCurrentEpisode() is not Episode episode)
            {
                LogOutput.Text = "No episode available to mark as watched.";
                return;
            }

            var logEpisode = episodeService.LogEpisodeWatched(CurrentShow!);
            if (HandleFailure(logEpisode)) return;

            UpdateNExtEpisodeInfo();
        }
        private void ChangeEpisodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentShow is null)
            {
                LogOutput.Text = "No show is currently selected.";
                return;
            }

            var picker = new ChangeEpisodeWindow(CurrentShow);
            bool? pickerResult = picker.ShowDialog();

            if (pickerResult == true && picker.selectedEpisode != null)
            {
                var result = episodeService.SetCurrentEpisode(picker.selectedEpisode);
                if (HandleFailure(result)) return;

                LoadSelectedShow();
                UpdateNExtEpisodeInfo();
            }
        }

        private void AddShowButton_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            string selectedFolder = dialog.SelectedPath;

            // Call the service method to get a sample video file
            var result = showService.GetFirstVideoFile(selectedFolder);
            if (HandleFailure(result)) return;

            string sampleFile = result.Data!;

            // Open your episode number picker window, passing the sampleFile for display
            var picker = new EpisodeNumberPickerWindow(sampleFile);
            bool? pickerResult = picker.ShowDialog();

            if (pickerResult == true)
            {
                //Retrieve user's numbering scheme from picker, then call AddShowFromFolder with it
                string numberingScheme = picker.SelectedRegex;

                var addShowResult = showService.AddShowFromFolder(selectedFolder, numberingScheme);
                if (HandleFailure(addShowResult)) return;

                LogOutput.Text = $"Show '{addShowResult.Data!.Name}' added successfully.";
                RefreshUI();
            }
        }
        private void RefreshUI()
        {
            LoadShowNames();
            LoadSelectedShow();
            bool hasShow = CurrentShow != null;

            WatchNextButton.IsEnabled = hasShow;
            MarkWatchedButton.IsEnabled = hasShow;
            ChangeEpisodeButton.IsEnabled = hasShow;

            UpdateNExtEpisodeInfo();
        }
        private void LoadSelectedShow()
        {
            var selectedName = ShowSelector.SelectedValue as string;
            if (string.IsNullOrWhiteSpace(selectedName))
            {
                CurrentShow = null;
                NextEpisodeInfo.Text = "No show selected.";
                return;
            }

            var result = showService.GetShowFromName(selectedName);
            if (HandleFailure(result))
            {
                CurrentShow = null;
                NextEpisodeInfo.Text = "No episode selected.";
                return;
            }

            CurrentShow = result.Data!;
            UpdateNExtEpisodeInfo();
        }

        private void LoadShowNames()
        {
            var names = showService.GetAllShowNames();
            if (HandleFailure(names)) return;
            if (names.Data!.Count() == 0)
            {
                ShowSelector.ItemsSource = new List<string> { "Please add a show" };
                ShowSelector.SelectedIndex = 0;
                ShowSelector.IsEnabled = false;
                return;
            }
            ShowSelector.ItemsSource = names.Data;
            ShowSelector.SelectedIndex = 0;
        }
        private Episode? GetCurrentEpisode()
        {
            if (CurrentShow == null) return null;
            
            return CurrentShow.Seasons
                .SelectMany(s => s.Episodes)
                .FirstOrDefault(e => e.FilePath == CurrentShow.CurrentEpisodePath);
        }
        private void UpdateNExtEpisodeInfo()
        {
            if (CurrentShow == null)
            {
                NextEpisodeInfo.Text = "No show selected.";
                return;
            }
            var nextEpisode = GetCurrentEpisode();

            if (nextEpisode == null)
            {
                NextEpisodeInfo.Text = "You’ve finished this show!";
                return;
            }

            var season = nextEpisode.Season.ToString("D2");
            var episode = nextEpisode.Number.ToString("D2");
            var minutes = (int)nextEpisode.Duration.TotalMinutes;
            var seconds = nextEpisode.Duration.Seconds;
            var duration = $"{minutes}m {seconds:D2}s";

            NextEpisodeInfo.Text = $"Season {season}, Episode {episode}\nDuration: {duration}";

        }
        private bool HandleFailure<T>(ResponseBody<T> response)
        {
            if (!response.Success)
            {
                LogOutput.Text = response.Message ?? "An unknown error occurred.";
                return true; // indicates failure
            }
            return false; // means it's OK to continue
        }
        private bool HandleFailure(ResponseBody response)
        {
            if (!response.Success)
            {
                LogOutput.Text = response.Message ?? "An unknown error occurred.";
                return true;
            }
            return false;
        }
        private void ShowSelector_ValueChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCurrentShowUI();
        }
        private void UpdateCurrentShowUI()
        {
            LoadSelectedShow();

            bool hasShow = CurrentShow != null;
            WatchNextButton.IsEnabled = hasShow;
            MarkWatchedButton.IsEnabled = hasShow;
            ChangeEpisodeButton.IsEnabled = hasShow;

            UpdateNExtEpisodeInfo();
        }



    }
}
