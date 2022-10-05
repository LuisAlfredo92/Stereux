using System.IO;
using Connections;
using Connections.Models;
using Connections.SongsDSTableAdapters;

namespace Stereux;

public class Playlist
{
    private readonly List<Song> _songs;
    private readonly int _middle;
    private readonly SongsTableAdapter _table;
    private readonly int _lastId;

    public Playlist()
    {
        //TODO: Take capacity from settings
        _table = new SongsTableAdapter();
        _lastId = (_table.GetLastSong().Rows[0] as SongsDS.SongsRow)!.Id;
        _songs = new List<Song>(Math.Clamp(11, 0, _lastId));
        _middle = (int)Math.Floor((double)(_songs.Capacity / 2));

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
        }
    }

    public Song CurrentSong()
    {
        if (_songs[_middle].AlbumCoverLocalPath != null && _songs[_middle].SongLocalPath != null &&
            File.Exists(_songs[_middle]!.SongLocalPath!))
            return _songs[_middle];

        _songs[_middle] = Downloader.Downloader.DownloadSongWithProgressBar(Properties.Settings.Default.DataPath, _songs[_middle]).Result;
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
            var downloadedSong = DownloadSong(newSong).Result;
            var index = _songs.IndexOf(newSong);
            if (index == -1) return;
            _songs[index] = downloadedSong;
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
            var downloadedSong = DownloadSong(newSong).Result;
            var index = _songs.IndexOf(newSong);
            if (index == -1) return;
            _songs[index] = downloadedSong;
        });

        return CurrentSong();
    }

    private async Task<Song> DownloadSong(Song song)
    {
        if (song.AlbumCoverLocalPath == null || song.SongLocalPath == null || !File.Exists(song.SongLocalPath))
            song = Downloader.Downloader.DownloadSong(Properties.Settings.Default.DataPath, song);
        return song;
    }

    public async void DownloadNextSongs()
    {
        _songs[6] = await DownloadSong(_songs[6]);
        _songs[4] = await DownloadSong(_songs[4]);
        for (var i = 7; i < 11; i++)
            _songs[i] = await DownloadSong(_songs[i]);
        for (var i = 0; i < 4; i++)
            _songs[i] = await DownloadSong(_songs[i]);
    }
}