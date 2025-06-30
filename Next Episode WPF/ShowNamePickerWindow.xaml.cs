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
    /// Interaction logic for ShowNamePickerWindow.xaml
    /// </summary>
    public partial class ShowNamePickerWindow : Window
    {
        public string SelectedShowName { get; private set; } = string.Empty;

        public ShowNamePickerWindow(IEnumerable<string> showNames)
        {   
            InitializeComponent();
            ShowNameComboBox.ItemsSource = showNames;
            if (showNames != null)
            {
                ShowNameComboBox.SelectedIndex = 0;

            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ShowNameComboBox.Text))
            {
                SelectedShowName = ShowNameComboBox.Text.Trim();
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please select or enter a show name.", "Validation");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
