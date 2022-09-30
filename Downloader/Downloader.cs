using System;
using System.ComponentModel;
using System.IO;
using Ookii.Dialogs.Wpf;
using System.Net.Http;
using System.Threading.Tasks;
using Connections.Models;

namespace Downloader
{
    public class Downloader
    {
        private static readonly HttpClient Client = new();

        private static readonly ProgressDialog ProgressDialog = new()
        {
            WindowTitle = "Downloading",
            Text = "Downloading song",
            Description = "Downloading song"
        };

        private static Task<HttpResponseMessage> _songTask, _albumCoverTask;

        public static async Task<Song> DownloadSong(string destRootFolder, Song song)
        {
            string destinationFolder = Path.Combine(destRootFolder, song.Id.ToString() ?? throw new InvalidOperationException()),
                destinationSongFile = Path.Combine(destinationFolder, song.Id + Path.GetExtension(song.SongURL)),
                destinationAlbumCover = Path.Combine(destinationFolder, song.Id + Path.GetExtension(song.AlbumCoverURL));
            if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
            Directory.CreateDirectory(destinationFolder);

            // Downloading song
            var songResponse = Client.GetAsync(song.SongURL).Result;
            await using var songStream = new FileStream(destinationSongFile, FileMode.CreateNew);
            songResponse.Content.CopyToAsync(songStream);

            // Downloading Album cover
            var albumCoverResponse = Client.GetAsync(song.AlbumCoverURL).Result;
            await using var albumCoverStream = new FileStream(destinationAlbumCover, FileMode.CreateNew);
            albumCoverResponse.Content.CopyToAsync(albumCoverStream);

            song.SongLocalPath = destinationSongFile;
            song.AlbumCoverLocalPath = destinationAlbumCover;

            return song;
        }

        public static async Task<Song> DownloadSongWithProgressBar(string destRootFolder, Song song)
        {
            string destinationFolder = Path.Combine(destRootFolder, song.Id.ToString() ?? throw new InvalidOperationException()),
                destinationSongFile = Path.Combine(destinationFolder, song.Id + Path.GetExtension(song.SongURL)),
                destinationAlbumCover = Path.Combine(destinationFolder, song.Id + Path.GetExtension(song.AlbumCoverURL));
            if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
            Directory.CreateDirectory(destinationFolder);

            ProgressDialog.Text = $"Downloading {song.Artists} - {song.Name}";
            ProgressDialog.DoWork += GetSongs_DoWork;

            _songTask = Client.GetAsync(song.SongURL);
            _albumCoverTask = Client.GetAsync(song.AlbumCoverURL);

            ProgressDialog.Show();

            await using var songStream = new FileStream(destinationSongFile, FileMode.CreateNew);
            await using var albumCoverStream = new FileStream(destinationAlbumCover, FileMode.CreateNew);

            _songTask.Result.Content.CopyToAsync(songStream);
            _albumCoverTask.Result.Content.CopyToAsync(albumCoverStream);

            song.SongLocalPath = destinationSongFile;
            song.AlbumCoverLocalPath = destinationAlbumCover;

            return song;

            static void GetSongs_DoWork(object? sender, DoWorkEventArgs e)
            {
                _songTask.Wait();
                ProgressDialog.ReportProgress(50, "", "Downloading album cover");
                _albumCoverTask.Wait();
                ProgressDialog.ReportProgress(100, "", "Song downloaded");
                ProgressDialog.Dispose();
            }
        }
    }
}