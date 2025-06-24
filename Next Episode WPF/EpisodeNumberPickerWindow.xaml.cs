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

            regexMap[Option1] = @"(?<!\w)[eE](\d+)";                            // E12
            regexMap[Option2] = @"[sS](\d{1,2})[\s\-]?[eE](\d+)";               // S01E12 or S01-E12
            regexMap[Option3] = @"\bEpisode[\s_]?(\d+)\b";                      // Episode 12 or Episode_12
            regexMap[Option4] = @"\b(\d{1,2})x(\d+)\b";                         // 1x12
            regexMap[Option5] = @"(?<=\D|^)(\d+)(?=\D|$)";                      // Number, but not inside 1080p etc
            regexMap[Option6] = @"\b[eE][pP](\d+)\b";                           // Ep12
            regexMap[Option7] = @"(?<=\s)-\s*(\d+)";                            // - 12 (but ignore stuff like Movie-1080p)


            UpdateRegexPreview();

        }

        private void AnyPresetOption_Checked(object sender, RoutedEventArgs e)
        {
            CustomRegexTextBox.Visibility = Visibility.Hidden;
            UpdateRegexPreview();
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
        private void CustomRegexTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CustomOption.IsChecked == true)
            {
                UpdateRegexPreview();
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string? selectedPattern = GetSelectedRegexPattern();

            if (string.IsNullOrWhiteSpace(selectedPattern))
            {
                MessageBox.Show("Please enter or select a regex pattern.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _ = new Regex(selectedPattern); // Validate regex
                SelectedRegex = selectedPattern;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid regex pattern:\n\n{ex.Message}", "Regex Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void UpdateRegexPreview()
        {
            string? pattern = GetSelectedRegexPattern();

            if (string.IsNullOrWhiteSpace(pattern))
            {
                RegexPreviewTextBlock.Text = "";
                return;
            }

            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                var match = regex.Match(SampleFileTextBlock.Text);

                if (match.Success)
                {
                    string matchedValue = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
                    RegexPreviewTextBlock.Text = $"✓ Match: {matchedValue}";
                    RegexPreviewTextBlock.Foreground = Brushes.Green;
                }
                else
                {
                    RegexPreviewTextBlock.Text = "⚠ No match found in sample filename.";
                    RegexPreviewTextBlock.Foreground = Brushes.OrangeRed;
                }
            }
            catch (Exception ex)
            {
                RegexPreviewTextBlock.Text = $"⚠ Invalid regex: {ex.Message}";
                RegexPreviewTextBlock.Foreground = Brushes.Red;
            }
        }
        private string? GetSelectedRegexPattern()
        {
            if (CustomOption.IsChecked == true)
            {
                string input = CustomRegexTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(input) || input == "enter the number scheme here")
                {
                    MessageBox.Show("Please enter a regex pattern.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                // If no capture group, wrap input safely
                if (!input.Contains("("))
                {
                    input = $@"(?<=\D|^)({input})(?=\D|$)";
                }

               return input;
            }

            var selected = regexMap.FirstOrDefault(pair => pair.Key.IsChecked == true);
            return selected.Key != null ? selected.Value : null;
        }


    }
}
