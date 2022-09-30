using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Connections.Models;
using Stereux.Settings;

namespace Stereux
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : Window
    {
        private bool _isPlaying = false;
        private readonly DrawingImage _playBrush, _pauseBrush;
        private Playlist? _playlist;

        private Song? _currentSong;

        private Song? CurrentSong
        {
            get => _currentSong;
            set
            {
                _currentSong = value;
                if (_currentSong is null) return;

                SongNameLabel.Content = CurrentSong!.Name;
                ArtistsNamesLabel.Content = CurrentSong!.Artists;
                SongInfoContainer.ChangeVars(CurrentSong!.Source, CurrentSong!.InfoURL);
            }
        }

        public Player()
        {
            InitializeComponent();

            if (Properties.Settings.Default.DataPath.Length < 1)
            {
                Properties.Settings.Default.DataPath =
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Stereux"
                        );
                Properties.Settings.Default.Save();
            }

            Directory.CreateDirectory(Properties.Settings.Default.DataPath);
            CreatePlaylist();

            _playBrush = (FindResource("PlayDrawingImage") as DrawingImage)!;
            _pauseBrush = (FindResource("PauseDrawingImage") as DrawingImage)!;
        }

        private void CreatePlaylist()
        {
            try
            {
                _playlist = new Playlist();
                CurrentSong = _playlist.CurrentSong();
            }
            catch (IndexOutOfRangeException)
            {
                if (MessageBox.Show("There isn't enough songs to make a playlist, go to settings to get more",
                        "Not enough songs", MessageBoxButton.OK, MessageBoxImage.Error) is MessageBoxResult.OK or MessageBoxResult.Cancel)
                    new SettingsWindow().Show();
                _playlist = null;
                CurrentSong = null;
            }
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
            => new SettingsWindow().Show();

        private void NextBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_playlist != null) CurrentSong = _playlist.NextSong();
            else CreatePlaylist();
        }

        private void PrevBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_playlist != null) CurrentSong = _playlist.PreviousSong();
            else CreatePlaylist();
        }

        private void PlayBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_playlist != null)
            {
                (PlayBtn.Content as Image)!.Source = _isPlaying ? _pauseBrush : _playBrush;
                _isPlaying = !_isPlaying;
            }
            else CreatePlaylist();
        }
    }
}