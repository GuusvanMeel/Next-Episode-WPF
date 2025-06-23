using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

        public HomeWindow(EpisodeService epservice, PlayerService playservice, ShowService showservice)
        {
            InitializeComponent();
            this.episodeService = epservice;
            this.playerService = playservice;
            this.showService = showservice;
            LoadShowNames();
        }

        private void LoadShowNames()
        {
            var names = showService.GetAllShowNames();
            if (names.Success == false)
            {
                LogOutput.Text = names.Message ?? "An Unknown Error ocurred";
                return;
            }
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

        private void ChangeEpisodeButton_Click(object sender, RoutedEventArgs e)
        {
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
            if (!result.Success)
            {
                LogOutput.Text = result.Message;
                return;
            }

            string sampleFile = result.Data!;

            // Open your episode number picker window, passing the sampleFile for display
            var picker = new EpisodeNumberPickerWindow(sampleFile);
            bool? pickerResult = picker.ShowDialog();

            if (pickerResult == true)
            {
                // Retrieve user's numbering scheme from picker, then call AddShowFromFolder with it
                //string numberingScheme = picker.NumberingScheme;

                //var addShowResult = showService.AddShowFromFolder(selectedFolder, numberingScheme);
                //if (!addShowResult.Success)
                //{
                //    LogOutput.Text = addShowResult.Message;
                //}
                //else
                //{
                //    LogOutput.Text = $"Show '{addShowResult.Data!.Name}' added successfully.";
                //    LoadShowNames(); // Refresh UI show list
                //}
            }
        }
    }
}
