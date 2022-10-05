using System.IO;
using Connections;
using Connections.Models;
using Connections.SongsDSTableAdapters;

namespace Stereux;

public class Playlist
{
    private readonly List<Song> _songs;
    private readonly SongsTableAdapter _table;
    private readonly int _lastId;

    public Playlist()
    {
        //TODO: Take capacity from settings
        _songs = new List<Song>(11);
        _table = new SongsTableAdapter();
        _lastId = (_table.GetLastSong().Rows[0] as SongsDS.SongsRow)!.Id;

        for (byte i = 0; i < 11; i++)
            do
            {
                var result = _table.GetSong(new Random().Next(1, _lastId + 1)).Rows[0] as SongsDS.SongsRow;
                _songs.Add(new Song(
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
                ));
                /*
                 * Songs aren't downloaded here because that would
                 * do startup extremely slow
                 */
            } while (_songs.Count(elem => elem.Equals(_songs[i])) > 1 && _lastId > 11);
    }

    public Song CurrentSong()
    {
        if (_songs[5].AlbumCoverLocalPath != null && _songs[5].SongLocalPath != null &&
            File.Exists(_songs[5]!.SongLocalPath!))
            return _songs[5];

        _songs[5] = Downloader.Downloader.DownloadSongWithProgressBar(Properties.Settings.Default.DataPath, _songs[5]).Result;
        return _songs[5];
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