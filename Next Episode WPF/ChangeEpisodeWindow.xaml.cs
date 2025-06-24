using Interfaces.Entities;
using System;
using System.Collections.Generic;
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

namespace Next_Episode_WPF
{
    public partial class ChangeEpisodeWindow : Window
    {
        public Episode? selectedEpisode { get; private set; }
        public ChangeEpisodeWindow(Show show)
        {
            InitializeComponent();
            EpisodeTreeView.ItemsSource = show.Seasons;
        }

        private void EpisodeTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Episode selected)
            {
                ConfirmButton.IsEnabled = true;
                selectedEpisode = selected;
            }
            else
            {
                ConfirmButton.IsEnabled = false;
                selectedEpisode = null;
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            selectedEpisode = EpisodeTreeView.SelectedItem as Episode;
            this.DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
