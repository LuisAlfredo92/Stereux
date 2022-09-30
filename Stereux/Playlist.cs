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
                /* I didn't download songs here because that will do
                 * startup very slow, so I chose to download them discretely
                 * when they're added to the playlist, or with a progress
                 * bar Dialog when they're needed like first play
                 */
            } while (_songs.Count(elem => elem.Equals(_songs[i])) > 1 && _lastId > 11);
    }

    public Song CurrentSong()
    {
        if (_songs[5].AlbumCoverLocalPath == null || _songs[5].SongLocalPath == null)
            _songs[5] = Downloader.Downloader.DownloadSongWithProgressBar(Properties.Settings.Default.DataPath, _songs[5]).Result;
        return _songs[5];
    }

    public Song NextSong()
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
        newSong = DownloadSong(newSong);

        _songs.Add(newSong);

        return CurrentSong();
    }

    public Song PreviousSong()
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
        newSong = DownloadSong(newSong);

        _songs.Insert(0, newSong);

        return CurrentSong();
    }

    private Song DownloadSong(Song song)
    {
        if (song.AlbumCoverLocalPath == null || song.SongLocalPath == null)
            song = Downloader.Downloader.DownloadSong(Properties.Settings.Default.DataPath, song).Result;
        return song;
    }
}