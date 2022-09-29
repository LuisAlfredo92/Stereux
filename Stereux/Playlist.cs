using Connections;
using Connections.Models;
using Connections.SongsDSTableAdapters;

namespace Stereux;

public class Playlist
{
    private List<Song> _songs;
    private SongsTableAdapter _table;
    private int _lastId;

    public Playlist()
    {
        //TODO: Take capacity from settings
        _songs = new List<Song>(11);
        _table = new SongsTableAdapter();
        var lastRow = _table.GetLastSong().Rows[0];
        if (lastRow is null) return;

        _lastId = ((lastRow as SongsDS.SongsRow)!).Id;
        for (byte i = 0; i < 11; i++)
            do
            {
                var result = _table.GetSong(new Random().Next(1, _lastId + 1)).Rows[0] as SongsDS.SongsRow;
                _songs[i] = new Song(
                    (Sources)result!.Source,
                    result.Name,
                    result.Artists,
                    result.AlbumCoverURL,
                    result.Genre,
                    result.InfoURL,
                    result.SongURL,
                    result.AlbumCoverLocalPath,
                    result.SongLocalPath
                );
            } while (_songs.Count(elem => elem.Equals(_songs[i])) > 1 && _lastId > 11);
    }

    public Song CurrentSong() => _songs[5];

    public Song NextSong()
    {
        _songs.RemoveAt(0);
        var result = _table.GetSong(new Random().Next(1, _lastId + 1)).Rows[0] as SongsDS.SongsRow;
        _songs.Add(new Song(
            (Sources)result!.Source,
            result.Name,
            result.Artists,
            result.AlbumCoverURL,
            result.Genre,
            result.InfoURL,
            result.SongURL,
            result.AlbumCoverLocalPath,
            result.SongLocalPath
        ));
        return _songs[5];
    }

    public Song PreviousSong()
    {
        _songs.RemoveAt(10);
        var result = _table.GetSong(new Random().Next(1, _lastId + 1)).Rows[0] as SongsDS.SongsRow;
        _songs.Insert(0, new Song(
            (Sources)result!.Source,
            result.Name,
            result.Artists,
            result.AlbumCoverURL,
            result.Genre,
            result.InfoURL,
            result.SongURL,
            result.AlbumCoverLocalPath,
            result.SongLocalPath
        ));
        return _songs[5];
    }
}