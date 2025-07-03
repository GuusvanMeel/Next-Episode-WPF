using Interfaces.Entities;
using Service;
using Service.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Next_Episode_WPF
{
   
    public partial class HomeWindow : Window
    {
        private readonly EpisodeService episodeService;

        private readonly PlayerService playerService;

        private readonly ShowService showService;
        private readonly ActivityService activityService;
        private readonly UserService userService;

        private Show? CurrentShow { get; set; }
        public HomeWindow(EpisodeService epservice, PlayerService playservice, ShowService showservice, ActivityService activityservice, UserService userservice)
        {
            InitializeComponent();
            this.episodeService = epservice;
            this.playerService = playservice;
            this.showService = showservice;
            this.activityService = activityservice;
            this.userService = userservice;
            RefreshUI();
            FillRecentActivity();

        }


  
        private void WatchNextButton_Click(object sender, RoutedEventArgs e)
        {
            Episode? currep = GetCurrentEpisode();
            if (currep == null)
            {
                LogOutput.Text = "No episode available to mark as watched.";
                return;
            }

            userService.UpdateEpisodesWatched(1);
            userService.IncreaseTimeWatched(currep.Duration);

            var startvideo = playerService.StartVideo(currep.FilePath);
            if (HandleFailure(startvideo)) return;

            var logEpisode = episodeService.LogEpisodeWatched(CurrentShow!);
            if (HandleFailure(logEpisode)) return;
            if (logEpisode.Data!.IsFinished)
            {
                userService.UpdateShowsWatched(1);
                CurrentShow = logEpisode.Data;
            }

            var logactivity = activityService.LogEpisodeWatched(currep.ShowName, currep.Season, currep.Number);
            if (HandleFailure(logactivity)) return;

            UpdateActivity(logactivity.Data!);

            RefreshUI();
        }
        private void MarkWatchedButton_Click(object sender, RoutedEventArgs e)
        {
            Episode? currep = GetCurrentEpisode();
            if (currep == null)
            {
                LogOutput.Text = "No episode available to mark as watched.";
                return;
            }

            userService.UpdateEpisodesWatched(1);
            userService.IncreaseTimeWatched(currep.Duration);
            
            var logEpisode = episodeService.LogEpisodeWatched(CurrentShow!);
            if (HandleFailure(logEpisode)) return;
            if (logEpisode.Data!.IsFinished)
            {
                userService.UpdateShowsWatched(1);
                CurrentShow = logEpisode.Data;
            }
            var logactivity = activityService.LogEpisodeWatched(currep.ShowName, currep.Season, currep.Number);
            if (HandleFailure(logactivity)) return;

            UpdateActivity(logactivity.Data!);
            RefreshUI();
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
                var names = showService.GetAllShowNamesFromApp();
                if (HandleFailure(names)) return;

                var namepicker = new ShowNamePickerWindow(names.Data!);
                bool? showpickerresult = namepicker.ShowDialog(); 

                if (showpickerresult == true)
                {


                    var addShowResult = showService.AddShowFromFolder(selectedFolder, numberingScheme, namepicker.SelectedShowName);
                    if (HandleFailure(addShowResult)) return;

                    LogOutput.Text = $"Show '{addShowResult.Data!.Name}' added successfully.";

                    var activitylog = activityService.AddedShow(addShowResult.Data!.Name);
                    if (HandleFailure(activitylog)) return;

                    UpdateActivity(activitylog.Data!);

                    RefreshUI();
                }
            }
        }
        private void RestartShowButton_Click(object sender, RoutedEventArgs e)
        {   if (CurrentShow == null) return;
            var result = showService.ResetShowProgress(CurrentShow);
            if (HandleFailure(result)) return;

            RefreshUI();
        }
        private void RefreshUI()
        {
            LoadShowNames();
            LoadSelectedShow();

            bool hasShow = CurrentShow != null && !CurrentShow.IsFinished;
            ChangeEpisodeButton.Click -= ChangeEpisodeButton_Click;
            ChangeEpisodeButton.Click -= RestartShowButton_Click;
            if (!hasShow)
            {
                WatchNextButton.Content = "Finished the show!";
                MarkWatchedButton.Content = "Please select a new show.";
                ChangeEpisodeButton.Content = "Or restart this show and watch again!";
                ChangeEpisodeButton.Click += RestartShowButton_Click;
            }
            else
            {
                // Reset buttons to default content when a valid show is selected
                WatchNextButton.Content = "▶ Watch the next episode";
                MarkWatchedButton.Content = "✔ Mark as watched";
                ChangeEpisodeButton.Content = "Change current episode";
                ChangeEpisodeButton.Click += ChangeEpisodeButton_Click;
            }

            WatchNextButton.IsEnabled = hasShow;
            MarkWatchedButton.IsEnabled = hasShow;
            //ChangeEpisodeButton.IsEnabled = hasShow;

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
            LoadPoster(CurrentShow.PosterFileName);
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


            NextEpisodeInfo.Text = UIFormatter.FormatNextEpisodeInfo(nextEpisode);

        }
      
        private void ShowSelector_ValueChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCurrentShowUI();
        }
        private void UpdateCurrentShowUI()
        {
            LoadSelectedShow();

            bool hasShow = CurrentShow != null && !CurrentShow.IsFinished;
            ChangeEpisodeButton.Click -= ChangeEpisodeButton_Click;
            ChangeEpisodeButton.Click -= RestartShowButton_Click;
            if (!hasShow)
            {
                WatchNextButton.Content = "Finished the show!";
                MarkWatchedButton.Content = "Please select a new show.";
                ChangeEpisodeButton.Content = "Or restart this show and watch again!";
                ChangeEpisodeButton.Click += RestartShowButton_Click;
            }
            else
            {
                // Reset buttons to default content when a valid show is selected
                WatchNextButton.Content = "▶ Watch the next episode";
                MarkWatchedButton.Content = "✔ Mark as watched";
                ChangeEpisodeButton.Content = "Change current episode";
                ChangeEpisodeButton.Click += ChangeEpisodeButton_Click;                
            }

            WatchNextButton.IsEnabled = hasShow;
            MarkWatchedButton.IsEnabled = hasShow;
            //ChangeEpisodeButton.IsEnabled = hasShow;

            UpdateNExtEpisodeInfo();
        }
        private void FillRecentActivity()
        {
            var activities = activityService.GetActivity();
            if (HandleFailure(activities)) return;
            if (activities.Data == null) return;
            foreach (ActivityLog a in activities.Data)
            {
                UpdateActivity(a);
            }

        }
        private void UpdateActivity(ActivityLog a)
        {
            RecentActivityPanel.Children.Insert(0,new TextBlock { Text = UIFormatter.FormatActivity(a) });
        }
        private void LoadPoster(string posterpath)
        {
            try
            {
                string postersDirectory = Path.Combine(AppContext.BaseDirectory, "data", "posters");
                string posterPath = Path.Combine(postersDirectory, posterpath);

                if (File.Exists(posterPath))
                {
                    PosterImage.Source = new BitmapImage(new Uri(posterPath, UriKind.Absolute));
                }
                else
                {
                    PosterImage.Source = null; // or set to a placeholder image if preferred
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load poster: " + ex.Message);
            }
        }

        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            var statsResponse = userService.GetUserStats();
            if (HandleFailure(statsResponse)) return;
            
                var statsWindow = new StatisticsWindow(statsResponse.Data!);
                statsWindow.ShowDialog();
            
        }
        private bool HandleFailure<T>(ResponseBody<T> response)
        {
            if (!response.Success || response.Data == null)
            {
                LogOutput.Text = response.Message ?? "An unknown error occurred.";
                return true;
            }
            return false;
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
    }
}
