using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Connections.Models;
using Downloader;
using Stereux.Settings;

namespace Stereux;

/// <summary>
/// Main window of the program. This plays the music with the Qr Codes
/// and credits to the artists
/// </summary>
public partial class Player
{
    /// <summary>
    /// Checks if a song is being playing or paused, and if it's muted
    /// </summary>
    private bool _isPlaying, _isMuted;

    /// <summary>
    /// The variable that saves the volume to restore it when clicking
    /// the mute button
    /// </summary>
    private float _tempVolume;

    /// <summary>
    /// The images that will be used to replace changing buttons like play / pause
    /// or muted / unmuted
    /// </summary>
    private readonly DrawingImage _playImage, _pauseImage, _volumeImage, _muteImage;

    /// <summary>
    /// The object that will play the music
    /// </summary>
    private readonly MediaPlayer _player;

    /// <summary>
    /// The object that updates the timers each second.
    /// </summary>
    private readonly DispatcherTimer _timer;

    /// <summary>
    /// The playlist that manages the songs and downloads.
    /// </summary>
    private Playlist? _playlist;

    /// <summary>
    /// The current song, the one that's being played.
    /// </summary>
    private Song? _currentSong;

    /// <summary>
    /// Gets or Sets the current song and updates all the information on the <see cref="Player">Player</see>.
    /// </summary>
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
            TagLib.File? file = null;
            var success = false;
            try
            {
                file = TagLib.File.Create(CurrentSong.SongLocalPath);
            }
            catch (Exception e)
            {
                for (var i = 0; i < 3; i++)
                {
                    if (success) continue;

                    Thread.Sleep(1000);
                    file = TagLib.File.Create(CurrentSong.SongLocalPath);
                    success = true;
                }

                if (!success)
                {
                    File.WriteAllText("Error.txt", $"{e.Message}\n\n{e.StackTrace}");
                    throw;
                }
            }
            var duration = file!.Properties.Duration;

            // Setting text
            SongTimeTextBlock.Text = $"{duration:mm\\:ss}";
            CurrentTimeTextBlock.Text = $"{TimeSpan.Zero:mm\\:ss}";

            // Setting slider
            TimeSlider.Maximum = duration.TotalSeconds;
            TimeSlider.Value = 0;

            // Setting Image
            if (CurrentSong.AlbumCoverLocalPath is not null &&
                CurrentSong.AlbumCoverLocalPath.Length != 0 &&
                File.Exists(CurrentSong.AlbumCoverLocalPath))
                SongImage.Source = new BitmapImage(new Uri(CurrentSong.AlbumCoverLocalPath, UriKind.Absolute));
            else
                SongImage.Source = (FindResource("Stereux_logoDrawingGroup") as DrawingImage)!;

            // Setting sound
            _player.Stop();
            _player.Open(new Uri(CurrentSong.SongLocalPath!));
            _player.Volume = VolumeSlider.Value / 100;
            if (_isPlaying) _player.Play();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    public Player()
    {
        try
        {
            //TODO: Add option to check updates on startup
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            if (Updater.CheckUpdates(fvi.FileVersion!))
            {
                var boxResult = MessageBox.Show("There's a new version available. Do you want to download it?",
                    "New version available",
                    MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
                if (boxResult != MessageBoxResult.Yes) return;

                // Thanks to https://stackoverflow.com/a/43232486/11756870
                /* I removed the OSX and Linux part since this program will
                 * be Windows exclusive
                 */
                var url = "https://github.com/LuisAlfredo92/Stereux/releases/latest";
                try
                {
                    Process.Start(url);
                }
                catch
                {
                    // hack because of this: https://github.com/dotnet/corefx/issues/10361
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
            }
            else
                MessageBox.Show("You have the latest version of Stereux", "No new versions", MessageBoxButton.OK,
                    MessageBoxImage.Information);

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
        catch (Exception ex)
        {
            File.AppendAllText("Error.txt", $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Updates the timers.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    private void UpdateTimers(object? sender, EventArgs e)
    {
        CurrentTimeTextBlock.Text = $"{_player.Position:mm\\:ss}";
        TimeSlider.Value = _player.Position.TotalSeconds;
    }

    /// <summary>
    /// Creates the playlist if it's possible, if not, shows a message
    /// </summary>
    private void CreatePlaylist()
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

    /// <summary>
    /// Shows the settings window when pressing Settings windows
    /// </summary>
    private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        => new SettingsWindow().Show();

    /// <summary>
    /// Gets the next song in <see cref="_playlist">playlist</see>, updates player
    /// and plays it, starting the download of a new song
    /// </summary>
    private async void NextBtn_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_playlist != null)
            {
                _player.Pause();
                BlockButtons();
                CurrentSong = await _playlist.NextSong();
                _player.Play();
            }
            else CreatePlaylist();
        }
        catch (Exception exception)
        {
            File.WriteAllText("Error.txt", $"{exception.Message}\n\n{exception.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Gets the previous song in <see cref="_playlist">playlist</see>, updates player
    /// and plays it, starting the download of a new song
    /// </summary>
    private async void PrevBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (_playlist != null)
        {
            _player.Pause();
            BlockButtons();
            CurrentSong = await _playlist.PreviousSong();
            _player.Play();
        }
        else CreatePlaylist();
    }

    /// <summary>
    /// Plays or pauses the current song
    /// </summary>
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

    /// <summary>
    /// Updates the volume when moving the <see cref="VolumeSlider">Volume slider </see>
    /// </summary>
    private void VolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _player.Volume = VolumeSlider.Value / 100;
        if (_player.Volume == 0) return;

        _isMuted = false;
        _tempVolume = 0;
        (VolumeBtn.Content as Image)!.Source = _volumeImage;
    }

    /// <summary>
    /// Mutes or unmutes the volume and updates the volume icon
    /// </summary>
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

    /// <summary>
    /// Updates the song time when moving the <see cref="TimeSlider">Time slider</see>
    /// </summary>
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

    private void BlockButtons()
    {
        NextBtn.IsEnabled = false;
        PrevBtn.IsEnabled = false;
        var enablerDispatcherTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3),
            IsEnabled = true
        };
        enablerDispatcherTimer.Tick += (_, _) =>
        {
            NextBtn.IsEnabled = true;
            PrevBtn.IsEnabled = true;
            enablerDispatcherTimer.Stop();
        };
    }
}