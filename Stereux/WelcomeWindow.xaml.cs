using System.IO;
using System.Windows;
using Stereux.Introduction;

namespace Stereux
{
    /// <summary>
    /// Lógica de interacción para Introduction.xaml
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        private readonly bool _isPlayerBeingShowed;
        private byte _page = 1;

        /// <summary>
        /// The total pages. Change it to 7 or more
        /// if you want to add another page
        /// </summary>
        private const byte TotalPages = 6;

        public WelcomeWindow() : this(false)
        {
        }

        public WelcomeWindow(bool isPlayerBeingShowed = false)
        {
            InitializeComponent();
            _isPlayerBeingShowed = isPlayerBeingShowed;
            if (Properties.Settings.Default.FirstTimeOpening || isPlayerBeingShowed) return;
            new Player().Show();
            Close();
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
                if (_page == 2 &&
                    (ContainerFrame.Content as Page2)!.GetSongsBtn.IsEnabled &&
                    MessageBox.Show("You haven't gotten any song. Are you sure you want to continue?", "Not songs",
                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
                    return;

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
                if (!_isPlayerBeingShowed)
                    new Player().Show();
                Close();
            }
        }
    }
}