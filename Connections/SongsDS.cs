using System.Collections.Generic;
using System.Linq;
using Connections.Models;

namespace Connections
{
}

namespace Connections.SongsDSTableAdapters
{
    /// <summary>
    /// The songs table adapter with custom methods.
    /// </summary>
    public partial class SongsTableAdapter
    {
        /// <summary>
        /// Inserts a song into the database.
        /// </summary>
        /// <param name="song">The song to be added.</param>
        /// <returns>The number of rows affected. 1 is correct</returns>
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
                song.SongLocalPath
            );
        }

        /// <summary>
        /// Inserts several songs into the database.
        /// </summary>
        /// <param name="songs">The songs.</param>
        /// <returns>he number of rows affected.</returns>
        public int InsertSong(List<Song> songs)
            => songs.Sum(InsertSong);

        /// <summary>
        /// Truncates the table and deletes all the songs from the database.
        /// </summary>
        /// <returns>An (useless) int.</returns>
        public int TruncateTable() =>
            TruncateTable(0, 0, "", "", null, "");
    }
}