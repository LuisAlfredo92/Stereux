using System.Windows;

namespace Stereux
{
    /// <summary>
    /// Lógica de interacción para Introduction.xaml
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        private byte _page = 1;

        /// <summary>
        /// The total pages. Change it to 4 or more
        /// if you want to add another page
        /// </summary>
        private const byte TotalPages = 5;

        public WelcomeWindow()
        {
            InitializeComponent();
        }

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            _page--;
            PageLabel.Content = $"{_page} / {TotalPages}";
            ContainerFrame.Source = new Uri($"Introduction/Page{_page}.xaml", UriKind.Relative);
            if (_page < 2)
                PreviousButton.IsEnabled = false;
            if (_page < TotalPages)
                NextButton.Content = "Next ↪";
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_page < TotalPages)
            {
                _page++;
                PageLabel.Content = $"{_page} / {TotalPages}";
                PreviousButton.IsEnabled = true;
                ContainerFrame.Source = new Uri($"Introduction/Page{_page}.xaml", UriKind.Relative);
                if (_page == TotalPages)
                    NextButton.Content = "Finish";
            }
            else
            {
                Properties.Settings.Default.FirstTimeOpening = false;
                Properties.Settings.Default.Save();
                new Player().Show();
                Close();
            }
        }
    }
}