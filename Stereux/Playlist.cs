using System.Diagnostics;
using Connections;
using Connections.Models;
using Connections.SongsDSTableAdapters;
using System.IO;
using System.Net.Http;
using System.Net.Http.Handlers;
using Ookii.Dialogs.Wpf;

namespace Stereux;

public class Playlist
{
    private readonly List<Song> _songs;
    private readonly int _middle;
    private readonly SongsTableAdapter _table;
    private readonly int _lastId;
    private Dictionary<Song, SongTasks?> _songTasksMap;
    private ProgressMessageHandler ph = new ProgressMessageHandler(new HttpClientHandler { AllowAutoRedirect = true });
    private HttpClient client;

    private readonly ProgressDialog _currentSongProgressDialog = new()
    {
        WindowTitle = "Downloading song",
        Text = "Downloading",
        Description = "Processing...",
        ShowCancelButton = false
    };

    public Playlist()
    {
        //TODO: Take capacity from settings
        _table = new SongsTableAdapter();
        _lastId = (_table.GetLastSong().Rows[0] as SongsDS.SongsRow)!.Id;
        _songs = new List<Song>(Math.Clamp(11, 0, _lastId));
        _middle = (int)Math.Floor((double)(_songs.Capacity / 2));
        _songTasksMap = new Dictionary<Song, SongTasks?>();
        ph.HttpReceiveProgress += (_, args) =>
        {
            Debug.WriteLine($"Download progress: {args.ProgressPercentage}%");
        };
        client = new HttpClient(ph) { Timeout = TimeSpan.FromSeconds(1000) };

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
            _songs.Add(newSong);
            _songTasksMap.Add(newSong, null);
        }

        DownloadSong(_middle);
    }

    public Song CurrentSong()
    {
        if (_songTasksMap[_songs[_middle]] is null)
            return _songs[_middle];

        var currentTasks = _songTasksMap[_songs[_middle]]!;
        if (currentTasks.DownloadSongTask.IsCompleted && currentTasks.DownloadCoverTask.IsCompleted &&
            currentTasks.CopySongTask.IsCompleted && currentTasks.CopyCoverTask.IsCompleted)
            return _songs[_middle];
        
        _currentSongProgressDialog.Show();
        currentTasks.DownloadSongTask.Wait();
        currentTasks.DownloadCoverTask.Wait();
        currentTasks.CopySongTask.Wait();
        currentTasks.CopyCoverTask.Wait();
        _currentSongProgressDialog.ReportProgress(100);
        _currentSongProgressDialog.Dispose();

        return _songs[_middle];
    }

    public async Task<Song?> NextSong()
    {
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
        Task.Run(() =>
        {
            //var downloadedSong = DownloadSong(newSong).Result;
            var index = _songs.IndexOf(newSong);
            if (index == -1) return;
            //_songs[index] = downloadedSong;
        });

        return CurrentSong();
    }

    public async Task<Song?> PreviousSong()
    {
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
        Task.Run(() =>
        {
            //var downloadedSong = DownloadSong(newSong).Result;
            var index = _songs.IndexOf(newSong);
            if (index == -1) return;
            //_songs[index] = downloadedSong;
        });

        return CurrentSong();
    }

    private void DownloadSong(int index)
    {
        var obtainedSong = _songs[index];

        if (obtainedSong.AlbumCoverLocalPath != null && obtainedSong.SongLocalPath != null && File.Exists(obtainedSong.SongLocalPath!))
            return;

        Downloader.DownloadSong(out var downloadSongTask, out var downloadCoverTask, out var copySongTask, out var copyCoverTask,
            ref client, Properties.Settings.Default.DataPath, ref obtainedSong);
        var tasks = new SongTasks(downloadSongTask, downloadCoverTask, copySongTask, copyCoverTask);
        _songTasksMap[obtainedSong] = tasks;
        _songs[index] = obtainedSong;
    }

    public void DownloadNextSongs()
    {
        for (var i = 0; i < _songs.Capacity; i++)
            DownloadSong(i);
    }
}