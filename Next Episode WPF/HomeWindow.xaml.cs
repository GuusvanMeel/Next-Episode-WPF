using DAL;
using Interfaces.Entities;
using Service;
using Service.Helper;
using Service.Orchestrator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace Next_Episode_WPF
{
   
    public partial class HomeWindow : Window
    {
        private readonly EpisodeService episodeService;

        private readonly PlayerService playerService;

        private readonly ShowService showService;
        private readonly ActivityService activityService;
        private readonly UserService userService;
        private readonly PlaybackOrchestration playbackOrchestration;

        private Show? CurrentShow { get; set; }
        public HomeWindow(EpisodeService epservice, PlayerService playservice, ShowService showservice, ActivityService activityservice, UserService userservice, PlaybackOrchestration playbackorchestration)
        {
            InitializeComponent();
            this.episodeService = epservice;
            this.playerService = playservice;
            this.showService = showservice;
            this.activityService = activityservice;
            this.userService = userservice;
            this.playbackOrchestration = playbackorchestration;
            RefreshUI();
            FillRecentActivity();

        }


  
        private void WatchNextButton_Click(object sender, RoutedEventArgs e)
        {
            var result = playbackOrchestration.WatchNextEpisode(CurrentShow!, true);
            ProcessWatchResult(result);
        }
        private void MarkWatchedButton_Click(object sender, RoutedEventArgs e)
        {
            var result = playbackOrchestration.WatchNextEpisode(CurrentShow!, false);
            ProcessWatchResult(result);
        }
        private void ProcessWatchResult(ResponseBody<WatchResult> result)
        {
            if (HandleFailure(result)) return;

            CurrentShow = result.Data!.UpdatedShow;
            UpdateActivity(result.Data.ActivityLog);
            UpdateButtonsUI();
            UpdateNExtEpisodeInfo();
        }
        private void ChangeEpisodeButton_Click(object sender, RoutedEventArgs e)
        {

            var picker = new ChangeEpisodeWindow(CurrentShow!);
            bool? pickerResult = picker.ShowDialog();

            if (pickerResult == true && picker.selectedEpisode != null)
            {
                var result = episodeService.SetCurrentEpisode(picker.selectedEpisode);
                if (HandleFailure(result)) return;

                CurrentShow = result.Data;
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
            var picker = new EpisodeNumberPickerWindow(sampleFile)
            {
                Owner = this, // sets HomeWindow as the owner
                WindowStartupLocation = WindowStartupLocation.CenterOwner // centers over owner
            };
            bool? pickerResult = picker.ShowDialog();

            if (pickerResult == true)
            {
                //Retrieve user's numbering scheme from picker, then call AddShowFromFolder with it
                string numberingScheme = picker.SelectedRegex;
                var names = showService.GetAllShowNamesFromApp();
                if (HandleFailure(names)) return;

                var namepicker = new ShowNamePickerWindow(names.Data!)
                {
                    Owner = this, // sets HomeWindow as the owner
                    WindowStartupLocation = WindowStartupLocation.CenterOwner // centers over owner
                };
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
            UpdateButtonsUI();
            UpdateNExtEpisodeInfo();
        }
        private void UpdateButtonsUI()
        {
            bool hasShow = CurrentShow != null;
            bool isFinished = CurrentShow?.IsFinished == true;

            ChangeEpisodeButton.Click -= ChangeEpisodeButton_Click;
            ChangeEpisodeButton.Click -= RestartShowButton_Click;

            if (!hasShow)
            {
                WatchNextButton.Content = "No show selected.";
                MarkWatchedButton.Content = "Please select a show.";
                ChangeEpisodeButton.Content = "Add a show first!";
                // Disable all actions if no show
                WatchNextButton.IsEnabled = false;
                MarkWatchedButton.IsEnabled = false;
                ChangeEpisodeButton.IsEnabled = false;
            }
            else if (isFinished)
            {
                WatchNextButton.Content = "Finished the show!";
                MarkWatchedButton.Content = "Please select a new show.";
                ChangeEpisodeButton.Content = "Or restart this show and watch again!";
                ChangeEpisodeButton.Click += RestartShowButton_Click;

                WatchNextButton.IsEnabled = false;
                MarkWatchedButton.IsEnabled = false;
                ChangeEpisodeButton.IsEnabled = true;
            }
            else // Show is selected and not finished
            {
                WatchNextButton.Content = "▶ Watch the next episode";
                MarkWatchedButton.Content = "✔ Mark as watched";
                ChangeEpisodeButton.Content = "Change current episode";
                ChangeEpisodeButton.Click += ChangeEpisodeButton_Click;

                WatchNextButton.IsEnabled = true;
                MarkWatchedButton.IsEnabled = true;
                ChangeEpisodeButton.IsEnabled = true;
            }
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
      
        private void UpdateNExtEpisodeInfo()
        {
            if (CurrentShow == null)
            {
                NextEpisodeInfo.Text = "No show selected.";
                return;
            }
            var nextEpisode = episodeService.GetCurrentEpisode(CurrentShow);

            if (nextEpisode.Data == null)
            {
                NextEpisodeInfo.Text = "You’ve finished this show!";
                return;
            }

            NextEpisodeInfo.Text = UIFormatter.FormatNextEpisodeInfo(nextEpisode.Data);
            NextEpisodeTitle.Text = nextEpisode.Data.Title;
            var progress = episodeService.GetWatchProgress(CurrentShow);
            if (progress.Success)
            {
                WatchPercentageLabel.Text = $"Watch Progress: {progress.Data:0.##}%";
                WatchProgressBar.Value = progress.Data;
                byte red = (byte)(255 - (progress.Data / 100.0 * 255));
                byte green = (byte)(progress.Data / 100.0 * 255);
                WatchProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(red, green, 0));
                var rounded = TimeSpan.FromSeconds(Math.Ceiling(CurrentShow.TimeWatched.TotalSeconds));
                WatchTimeLabel.Text = $"Time Watched: {rounded.Hours}h {rounded.Minutes}m {rounded.Seconds}s";
            }
            else
            {
                WatchPercentageLabel.Text = "Watch Progress: N/A";
                WatchTimeLabel.Text = $"Time Watched: N/A";
            }

        }
      
        private void ShowSelector_ValueChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSelectedShow();
            UpdateButtonsUI();
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

            var statsWindow = new StatisticsWindow(statsResponse.Data!)
            {
                Owner = this, // sets HomeWindow as the owner
                WindowStartupLocation = WindowStartupLocation.CenterOwner // centers over owner
            };
                statsWindow.ShowDialog();
            
        }
        public bool HandleFailure<T>(ResponseBody<T> response)
        {
            if (!response.Success || response.Data == null)
            {
                LogOutput.Text = response.Message ?? "An unknown error occurred.";
                return true;
            }
            return false;
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(new(new SettingsJSONRepo(new FileLogger()),new FileLogger()), new(new FileLogger(), new UserStatsJSONRepo(new FileLogger())))
            {
                Owner = this, // sets HomeWindow as the owner
                WindowStartupLocation = WindowStartupLocation.CenterOwner // centers over owner
            };
            settingsWindow.ShowDialog();
        }

    }
}
