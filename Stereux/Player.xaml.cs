using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Connections.Models;
using Stereux.Settings;

namespace Stereux
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player
    {
        private bool _isPlaying, _isMuted;
        private float _tempVolume;
        private readonly DrawingImage _playImage, _pauseImage, _volumeImage, _muteImage;
        private readonly MediaPlayer _player;
        private readonly DispatcherTimer _timer;
        private Playlist? _playlist;

        private Song? _currentSong;

        private Song? CurrentSong
        {
            get => _currentSong;
            set
            {
                _currentSong = value;
                if (_currentSong is null) return;

                // Setting labels
                SongNameLabel.Content = CurrentSong!.Name;
                SongNameLabel.BeginAnimation(MarginProperty,
                    CurrentSong!.Name.Length > 25
                        ? new ThicknessAnimation()
                        {
                            From = new Thickness(160, 0, 0, 0),
                            To = new Thickness(160 - 10 * (CurrentSong.Name.Length - 25), 0, 0, 0),
                            AutoReverse = true,
                            Duration = Duration.Forever,
                        }
                        : null);

                ArtistsNamesLabel.Content = CurrentSong!.Artists;
                ArtistsNamesLabel.BeginAnimation(MarginProperty,
                    CurrentSong!.Artists.Length > 45
                        ? new ThicknessAnimation()
                        {
                            From = new Thickness(160, 30, 0, 0),
                            To = new Thickness(160 - 5 * (CurrentSong.Name.Length - 45), 30, 0, 0),
                            AutoReverse = true,
                            Duration = Duration.Forever,
                        }
                        : null);

                SongInfoContainer.ChangeVars(CurrentSong!.Source, CurrentSong!.InfoURL);

                // Setting time
                var file = TagLib.File.Create(CurrentSong.SongLocalPath);
                var duration = file.Properties.Duration;
                // Setting text
                SongTimeTextBlock.Text = $"{duration:mm\\:ss}";
                CurrentTimeTextBlock.Text = $"{TimeSpan.Zero:mm\\:ss}";
                // Setting slider
                TimeSlider.Maximum = duration.TotalSeconds;
                TimeSlider.Value = 0;

                // Setting Image
                // TODO: Check if song file has already an image and use that one
                if (CurrentSong.AlbumCoverLocalPath != null)
                    SongImage.Source = new BitmapImage(new Uri(CurrentSong.AlbumCoverLocalPath, UriKind.Absolute));
                else
                    SongImage.Source = (FindResource("Stereux_logoDrawingGroup") as DrawingImage)!;

                // Setting sound
                _player.Stop();
                _player.Open(new Uri(CurrentSong.SongLocalPath!));
                // TODO: Make this with log so it's more natural
                _player.Volume = VolumeSlider.Value / 100;
                if (_isPlaying) _player.Play();
            }
        }

        public Player()
        {
            // TODO: Uncomment this to enable the updater
            //var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            //var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            //if (Updater.CheckUpdates(fvi.FileVersion!).Result)
            //{
            //    var boxResult = MessageBox.Show("There's a new version available. Do you want to download it?", "New version available",
            //        MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
            //    if (boxResult == MessageBoxResult.Yes)
            //    {
            //        System.Diagnostics.Process.Start("https://github.com/LuisAlfredo92/Stereux/Releases/latest");
            //        Close();
            //    }
            //}

            _player = new MediaPlayer();
            _playImage = (FindResource("PlayDrawingImage") as DrawingImage)!;
            _pauseImage = (FindResource("PauseDrawingImage") as DrawingImage)!;
            _volumeImage = (FindResource("VolumeDrawingImage") as DrawingImage)!;
            _muteImage = (FindResource("MuteDrawingImage") as DrawingImage)!;

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

            if (!Directory.Exists(Properties.Settings.Default.DataPath))
                Directory.CreateDirectory(Properties.Settings.Default.DataPath);

            CreatePlaylist();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += UpdateTimers;
        }

        private void UpdateTimers(object? sender, EventArgs e)
        {
            CurrentTimeTextBlock.Text = $"{_player.Position:mm\\:ss}";
            TimeSlider.Value = _player.Position.TotalSeconds;
        }

        private async void CreatePlaylist()
        {
            try
            {
                _playlist = new Playlist();
                CurrentSong = _playlist.CurrentSong();
                Task.Run(() => _playlist.DownloadNextSongs());
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

        private async void NextBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_playlist != null)
            {
                _player.Pause();
                CurrentSong = await _playlist.NextSong();
                _player.Play();
            }
            else CreatePlaylist();
        }

        private async void PrevBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_playlist != null)
            {
                _player.Pause();
                CurrentSong = await _playlist.PreviousSong();
                _player.Play();
            }
            else CreatePlaylist();
        }

        private void PlayBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_playlist != null && _currentSong is not null)
            {
                _isPlaying = !_isPlaying;
                (PlayBtn.Content as Image)!.Source = _isPlaying ? _pauseImage : _playImage;

                CurrentTimeTextBlock.Text = $"{_player.Position:mm\\:ss}";
                TimeSlider.Value = _player.Position.TotalSeconds;
                if (_isPlaying)
                {
                    _player.Play();
                    _timer.Start();
                }
                else
                {
                    _player.Pause();
                    _timer.Stop();
                }
            }
            else CreatePlaylist();
        }

        // TODO: Make this with log so it's more natural
        private void VolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _player.Volume = VolumeSlider.Value / 100;
            if (_player.Volume == 0) return;
            _isMuted = false;
            _tempVolume = 0;
            (VolumeBtn.Content as Image)!.Source = _volumeImage;
        }

        private void VolumeBtn_OnClick(object sender, RoutedEventArgs e)
        {
            _isMuted = !_isMuted;
            (VolumeBtn.Content as Image)!.Source = _isMuted ? _muteImage : _volumeImage;
            if (_isMuted)
            {
                _tempVolume = (float)_player.Volume;
                _player.Volume = 0;
                VolumeSlider.Value = 0;
            }
            else
            {
                _player.Volume = _tempVolume;
                VolumeSlider.Value = _tempVolume * 100;
            }
        }

        private void TimeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            _player.Position = TimeSpan.FromSeconds(TimeSlider.Value);
            CurrentTimeTextBlock.Text = $"{_player.Position:mm\\:ss}";

            if (!(Math.Abs(TimeSlider.Value - TimeSlider.Maximum) < 0.5)) return;
            _player.Stop();
            TimeSlider.Value = 0;
            CurrentSong = _playlist!.NextSong().Result;
            if (_isPlaying) _player.Play();
        }
    }
}