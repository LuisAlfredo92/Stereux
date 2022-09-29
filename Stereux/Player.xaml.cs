using System.IO;
using System.Windows;
using Connections.Models;
using Stereux.Settings;

namespace Stereux
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : Window
    {
        private Playlist _playlist;

        private Song _currentSong;

        private Song CurrentSong
        {
            get => _currentSong;
            set
            {
                _currentSong = value;
                songNameLabel.Content = CurrentSong.Name;
                artistsNamesLabel.Content = CurrentSong.Artists;
                SongInfoContainer.ChangeVars(CurrentSong.Source, CurrentSong.InfoURL);
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

            _playlist = new Playlist();
            CurrentSong = _playlist.CurrentSong();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().Show();
        }

        private void NextBtn_OnClick(object sender, RoutedEventArgs e)
            => CurrentSong = _playlist.NextSong();

        private void PrevBtn_OnClick(object sender, RoutedEventArgs e)
            => CurrentSong = _playlist.PreviousSong();
    }
}