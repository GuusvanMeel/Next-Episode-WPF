using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Interfaces.Entities;
using Microsoft.Win32;
using Service;
using Service.Helper;

namespace Next_Episode_WPF
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService settingService;
        private readonly UserService userService;
        public SettingsWindow(SettingsService _settingservice, UserService _userservice)
        {
            InitializeComponent();
            this.settingService = _settingservice;
            this.userService = _userservice;
            var settings = settingService.GetCurrentSettings();
            if(settings.Success && settings.Data != null)
            {
                SetCurrentSettings(settings.Data);
            }
            
        }

        private void SelectVideoPlayer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            dialog.Title = "Select your video player executable";

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.FileName;
                SelectedPathText.Text = $"Selected: {selectedPath}";

                if (File.Exists(selectedPath) && selectedPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    ValidationText.Text = "✅ Valid executable found.";
                    ValidationText.Foreground = System.Windows.Media.Brushes.Green;

                    settingService.SetVideoPlayer(selectedPath);
                }
                else
                {
                    ValidationText.Text = "❌ Not a valid executable.";
                    ValidationText.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            
        }
        private void SetCurrentSettings(AppSettings settings)
        {   if(settings.VideoPlayerPath != null)
            {
                SelectedPathText.Text = "";
                ValidationText.Text = settings.VideoPlayerPath;
            }
            
        }

        private void ResetStatistics_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("Are you sure you want to reset all statistics? This cannot be undone.",
                                                "Confirm Reset",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var resetResult = userService.ResetUserStats(); // Assuming this exists in your service

                if (resetResult.Success)
                {
                    ValidationText.Text = "✅ Statistics reset successfully.";
                    ValidationText.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    ValidationText.Text = $"❌ Failed to reset statistics: {resetResult.Message}";
                    ValidationText.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
        }
    }
}
