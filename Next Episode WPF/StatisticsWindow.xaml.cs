using Interfaces.Entities;
using System.Windows;

namespace Next_Episode_WPF
{
    public partial class StatisticsWindow : Window
    {
        private readonly UserStats stats;

        public StatisticsWindow(UserStats userStats)
        {
            InitializeComponent();
            stats = userStats;

            LoadStats();
        }

        private void LoadStats()
        {
            TotalTimeWatchedLabel.Text = stats.TotalTimeWatched.ToString();
            EpisodesWatchedLabel.Text = stats.EpsWatched.ToString();
            ShowsCompletedLabel.Text = stats.ShowsCompleted.ToString();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
