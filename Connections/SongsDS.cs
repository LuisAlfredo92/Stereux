using System.Collections.Generic;
using System.Linq;
using Connections.Models;

namespace Connections
{
}

namespace Connections
{
}

namespace Connections
{
}

namespace Connections
{
}

namespace Connections.SongsDSTableAdapters
{
    public partial class SongsTableAdapter
    {
        public int InsertSong(Song song)
        {
            return InsertSong(
                (int)song.Source,
                song.Name,
                song.Artists,
                song.AlbumCoverURL,
                song.Genre,
                song.InfoURL,
                song.SongURL,
                song.AlbumCoverLocalPath,
                song.SongLocalPath,
                song.QrCodeLocalPath
            );
        }

        public int InsertSong(List<Song> songs)
            => songs.Sum(InsertSong);

        public int TruncateTable() =>
            TruncateTable(0, 0, "", "", null, "");
    }
}