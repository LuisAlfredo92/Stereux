using System.IO;
using Connections;
using Connections.Models;
using Connections.SongsDSTableAdapters;
using Ookii.Dialogs.Wpf;

namespace Stereux;

/// <summary>
/// The playlist that contains the songs that will be played
/// or were played
/// </summary>
public class Playlist
{
    /// <summary>
    /// The song state (If is being downloaded or not).
    /// </summary>
    private class SongState
    {
        /// <summary>
        /// Gets or Sets a value indicating whether a song is being downloaded.
        /// </summary>
        public bool IsBeingDownloaded { get; set; }

        /// <summary>
        /// Gets the download song task.
        /// </summary>
        public Task? SongTask { get; }

        /// <summary>
        /// Gets the download cover task.
        /// </summary>
        public Task? CoverTask { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SongState"/> class
        /// not being downloaded.
        /// </summary>
        public SongState()
        {
            IsBeingDownloaded = false;
            SongTask = null;
            CoverTask = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SongState"/> class
        /// being downloaded.
        /// </summary>
        /// <param name="songTask">The song task.</param>
        /// <param name="coverTask">The cover task.</param>
        public SongState(Task songTask, Task coverTask)
        {
            IsBeingDownloaded = true;
            SongTask = songTask;
            CoverTask = coverTask;
        }
    }

    /// <summary>
    /// The songs.
    /// </summary>
    private readonly List<Song> _songs;

    /// <summary>
    /// The middle value of the play list.
    /// </summary>
    private readonly int _middle;

    /// <summary>
    /// The songs table.
    /// </summary>
    private readonly SongsTableAdapter _table;

    /// <summary>
    /// The last id of the songs table.
    /// </summary>
    private readonly int _lastId;

    /// <summary>
    /// Dictionary that associates Songs with their download tasks
    /// </summary>
    private readonly Dictionary<Song, SongState> _songsTasks;

    /// <summary>
    /// The downloading song progress dialog.
    /// </summary>
    private readonly ProgressDialog _downloadingSongProgressDialog = new()
    {
        ShowCancelButton = false,
        Description = "Please be patient",
        Text = "Downloading song",
        ShowTimeRemaining = false,
        WindowTitle = "Downloading..."
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="Playlist"/> class.
    /// </summary>
    public Playlist()
    {
        //TODO: Take capacity from settings
        _table = new SongsTableAdapter();
        _lastId = (_table.GetLastSong().Rows[0] as SongsDS.SongsRow)!.Id;
        _songs = new List<Song>(Math.Clamp(11, 0, _lastId));
        _middle = (int)Math.Floor((double)(_songs.Capacity / 2));
        _songsTasks = new Dictionary<Song, SongState>();

        for (byte i = 0; i < Math.Clamp(11, 0, _lastId); i++)
        {
            Song newSong;
            do
            {
                var result = _table.GetSong(new Random().Next(1, _lastId + 1)).Rows[0] as SongsDS.SongsRow;
                newSong = new Song(
                    result!.Id,
                    (Sources)result!.Source,
                    result.Name,
                    result.Artists,
                    result.AlbumCoverURL,
                    result.Genre,
                    result.InfoURL,
                    result.SongURL,
                    result.IsAlbumCoverLocalPathNull() ? null : result.AlbumCoverLocalPath,
                    result.IsSongLocalPathNull() ? null : result.SongLocalPath
                );
            } while (_songs.Contains(newSong));
            /* Songs aren't downloaded here because that would
             * do startup extremely slow */
            _songs.Add(newSong);
            _songsTasks.Add(newSong, new SongState());
        }
    }

    /// <summary>
    /// Gets the current song (in the middle of the play list).
    /// </summary>
    /// <returns>The middle Song.</returns>
    public Song CurrentSong()
    {
        if (_songs[_middle].AlbumCoverLocalPath != null && _songs[_middle].SongLocalPath != null &&
            File.Exists(_songs[_middle]!.SongLocalPath!))
            return _songs[_middle];

        if (_songsTasks[_songs[_middle]].IsBeingDownloaded)
        {
            _downloadingSongProgressDialog.ShowDialog();
            do Task.Delay(500);
            while (_songsTasks[_songs[_middle]]!.SongTask is null);
            _songsTasks[_songs[_middle]]!.SongTask!.Wait();

            do Task.Delay(500);
            while (_songsTasks[_songs[_middle]]!.CoverTask is null);
            _songsTasks[_songs[_middle]]!.CoverTask!.Wait();

            _downloadingSongProgressDialog.Dispose();
            return _songs[_middle];
        }
        _songs[_middle] = Downloader.Downloader.DownloadSongWithProgressBar(Properties.Settings.Default.DataPath, _songs[_middle]).Result;
        return _songs[_middle];
    }

    /// <summary>
    /// Moves the play list one song adding and removing one.
    /// </summary>
    /// <returns><![CDATA[Task<Song?>]]>The current song already moved on the list</returns>
    public async Task<Song?> NextSong()
    {
        _songsTasks.Remove(_songs[0]);
        _songs.RemoveAt(0);
        var result = _table.GetSong(new Random().Next(1, _lastId + 1)).Rows[0] as SongsDS.SongsRow;
        var newSong = new Song(
            result!.Id,
            (Sources)result!.Source,
            result.Name,
            result.Artists,
            result.AlbumCoverURL,
            result.Genre,
            result.InfoURL,
            result.SongURL,
            result.IsAlbumCoverLocalPathNull() ? null : result.AlbumCoverLocalPath,
            result.IsSongLocalPathNull() ? null : result.SongLocalPath
        );
        _songs.Add(newSong);
        if (!_songsTasks.TryAdd(newSong, new SongState()))
            _songsTasks[newSong] = new SongState();
        Task.Run(() =>
        {
            var downloadedSong = DownloadSong(newSong).Result;
            var index = _songs.IndexOf(newSong);
            if (index == -1) return;
            _songs[index] = downloadedSong;
        });

        return CurrentSong();
    }

    /// <summary>
    /// Moves the play list one song adding and removing one.
    /// </summary>
    /// <returns><![CDATA[Task<Song?>]]>The current song already moved on the list</returns>
    public async Task<Song?> PreviousSong()
    {
        _songsTasks.Remove(_songs[10]);
        _songs.RemoveAt(10);
        var result = _table.GetSong(new Random().Next(1, _lastId + 1)).Rows[0] as SongsDS.SongsRow;
        var newSong = new Song(
            result!.Id,
            (Sources)result!.Source,
            result.Name,
            result.Artists,
            result.AlbumCoverURL,
            result.Genre,
            result.InfoURL,
            result.SongURL,
            result.IsAlbumCoverLocalPathNull() ? null : result.AlbumCoverLocalPath,
            result.IsSongLocalPathNull() ? null : result.SongLocalPath
        );
        _songs.Insert(0, newSong);
        if (!_songsTasks.TryAdd(newSong, new SongState()))
            _songsTasks[newSong] = new SongState();
        Task.Run(() =>
        {
            var downloadedSong = DownloadSong(newSong).Result;
            var index = _songs.IndexOf(newSong);
            if (index == -1) return;
            _songs[index] = downloadedSong;
        });

        return CurrentSong();
    }

    /// <summary>
    /// Downloads the song asynchronously.
    /// </summary>
    /// <param name="song">The song to be downloaded.</param>
    /// <returns><![CDATA[Task<Song>]]>The song with their local paths aiming to the local file</returns>
    private async Task<Song> DownloadSong(Song song)
    {
        if (song.AlbumCoverLocalPath != null && song.SongLocalPath != null &&
            File.Exists(song.SongLocalPath)) return song;

        _songsTasks[song].IsBeingDownloaded = true;
        song = Downloader.Downloader.DownloadSong(Properties.Settings.Default.DataPath, song, out var songTask, out var coverTask);
        _songsTasks[song] = new SongState(songTask, coverTask);

        return song;
    }

    /// <summary>
    /// Downloads the immediately next and previous song so the user can change to them
    /// quickly if they want to, then downloads the rest of the songs
    /// </summary>
    public async void DownloadNextSongs()
    {
        _songs[6] = await DownloadSong(_songs[_middle + 1]);
        _songs[4] = await DownloadSong(_songs[_middle - 1]);
        for (var i = _middle + 2; i < _songs.Capacity; i++)
            _songs[i] = await DownloadSong(_songs[i]);
        for (var i = _middle - 2; i >= 0; i--)
            _songs[i] = await DownloadSong(_songs[i]);
    }
}