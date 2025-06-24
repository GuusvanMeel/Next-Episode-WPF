using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Next_Episode_WPF
{
    /// <summary>
    /// Interaction logic for EpisodeNumberPickerWindow.xaml
    /// </summary>
    public partial class EpisodeNumberPickerWindow : Window
    {
        private readonly Dictionary<RadioButton, string> regexMap = new();

        public string SelectedRegex { get; private set; } = string.Empty;

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

            regexMap[Option1] = @"[eE](\d{1,3})";               // E12
            regexMap[Option2] = @"[sS](\d{1,2})[eE](\d{1,3})";   // S01E12
            regexMap[Option3] = @"Episode[\s_]?(\d{1,3})";       // Episode 12
            regexMap[Option4] = @"(\d{1,2})x(\d{1,3})";          // 1x12
            regexMap[Option5] = @"(?<!\d)(\d{1,3})(?!\d)";       // Just a number
            regexMap[Option6] = @"[eE][pP](\d{1,3})";            // EP12
            regexMap[Option7] = @"-\s*(\d{1,3})";                // - 12
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
            string selectedPattern;

            if (CustomOption.IsChecked == true)
            {
                string input = CustomRegexTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(input) || input == "enter the number scheme here")
                {
                    MessageBox.Show("Please enter a regex pattern.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                selectedPattern = input;
            }
            else
            {
                var selected = regexMap.FirstOrDefault(pair => pair.Key.IsChecked == true);
                if (selected.Key == null)
                {
                    MessageBox.Show("Please select a numbering scheme.", "Missing Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                selectedPattern = selected.Value;
            }

            try
            {
                _ = new Regex(selectedPattern); // single point of validation
                SelectedRegex = selectedPattern;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid regex pattern:\n\n" + ex.Message, "Regex Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
