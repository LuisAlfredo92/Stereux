using Connections.Models;
using System.IO;
using System.Net.Http;

namespace Stereux
{
    public class Downloader
    {
        public static void DownloadSong(out Task<HttpResponseMessage> downloadSongTask,
            out Task<HttpResponseMessage> downloadCoverTask, out Task copySongTask, out Task copyCoverTask,
            ref HttpClient client, string destRootFolder, ref Song song)
        {
            string destinationFolder = Path.Combine(destRootFolder, song.Id.ToString() ?? throw new ArgumentException("Id of the song can't be null")),
                destinationSongFile = Path.Combine(destinationFolder, song.Id + Path.GetExtension(song.SongURL)),
                destinationAlbumCover = Path.Combine(destinationFolder, song.Id + Path.GetExtension(song.AlbumCoverURL));
            if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
            Directory.CreateDirectory(destinationFolder);

            // TODO: Handle "System.InvalidOperationException: 'An invalid request URI was provided. Either the request URI must be an absolute URI or BaseAddress must be set.'"
            downloadSongTask = client.GetAsync(song.SongURL);
            downloadCoverTask = client.GetAsync(song.AlbumCoverURL);

            using var songStream = new FileStream(destinationSongFile, FileMode.Create);
            using var albumCoverStream = new FileStream(destinationAlbumCover, FileMode.Create);

            var songLocalTask = downloadCoverTask;
            copySongTask = Task.Run(() => songLocalTask.Result.Content.CopyToAsync(albumCoverStream));
            var coverLocalTask = downloadSongTask;
            copyCoverTask = Task.Run(() => coverLocalTask.Result.Content.CopyToAsync(songStream));

            song.AlbumCoverLocalPath = destinationAlbumCover;
            song.SongLocalPath = destinationSongFile;
        }
    }
}