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
    /// <summary>
    /// Interaction logic for EpisodeNumberPickerWindow.xaml
    /// </summary>
    public partial class EpisodeNumberPickerWindow : Window
    {
        public EpisodeNumberPickerWindow(string sampleFile)
        {
            InitializeComponent();
            SampleFileTextBlock.Text = sampleFile;
            Option1.IsChecked = true;

            // Hook events
            Option1.Checked += AnyPresetOption_Checked;
            Option2.Checked += AnyPresetOption_Checked;
            Option3.Checked += AnyPresetOption_Checked;
            Option4.Checked += AnyPresetOption_Checked;
            Option5.Checked += AnyPresetOption_Checked;
            Option6.Checked += AnyPresetOption_Checked;
            Option7.Checked += AnyPresetOption_Checked;
            CustomOption.Checked += CustomOption_Checked;
        }
        private void AnyPresetOption_Checked(object sender, RoutedEventArgs e)
        {
            CustomRegexTextBox.Visibility = Visibility.Hidden;
        }

        private void CustomOption_Checked(object sender, RoutedEventArgs e)
        {
            CustomRegexTextBox.Visibility = Visibility.Visible;
            CustomRegexTextBox.Text = "enter the number scheme here";
            CustomRegexTextBox.Foreground = Brushes.Gray;
        }
        private void CustomRegexTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
                CustomRegexTextBox.Text = "";
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
