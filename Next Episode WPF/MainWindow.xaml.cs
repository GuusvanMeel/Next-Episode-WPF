using Service;
using System.Windows;

namespace Next_Episode_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ShowService _showService;

        public MainWindow(ShowService showService)
        {
            InitializeComponent();
            _showService = showService;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}