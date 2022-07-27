using Connections.Models;

namespace Connections.Data.SongsDataSetTableAdapters
{


    public partial class SongsTableAdapter
    {
        /// <summary>
        /// Inserts a song with an Song object
        /// </summary>
        /// <param name="song">The song to be added, it can be some properties null</param>
        /// <returns>Numbers of rows affected, then 1 if success</returns>
        public int InsertSong(Song song) =>
            InsertSong(
                (int)song.Source,
                song.Name,
                song.Artists,
                song.AlbumCoverLink,
                song.Genre,
                song.InfoLink,
                song.SongLink,
                song.AlbumCoverLocalPath,
                song.SongLocalPath,
                song.QrCodeLocalPath);
    }
}
