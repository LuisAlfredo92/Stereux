using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Ookii.Dialogs.Wpf;
using System.Net.Http;
using System.Threading.Tasks;
using Connections.Models;
using System.Net.Http.Handlers;
using System.Threading;

namespace Downloader
{
    public class Downloader
    {
        private static readonly ProgressDialog DownloadDialog = new()
        {
            WindowTitle = "Downloading",
            Text = "Downloading song",
            Description = "Downloading song",
            ShowCancelButton = false,
            ShowTimeRemaining = true
        };

        private static Task<HttpResponseMessage> _songWithDialogTask, _albumCoverWithDialogTask, _songTask, _albumTask;
        private static int _songProgress, _albumProgress;

        public static Song DownloadSong(string destRootFolder, Song song, out Task refSongTask, out Task refCoverTask)
        {
            string destinationFolder = Path.Combine(destRootFolder, song.Id.ToString() ?? throw new InvalidOperationException()),
                destinationSongFile = Path.Combine(destinationFolder, song.Id + Path.GetExtension(song.SongURL)),
                destinationAlbumCover = Path.Combine(destinationFolder, song.Id + Path.GetExtension(song.AlbumCoverURL));
            if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
            Directory.CreateDirectory(destinationFolder);

            // Web client creation
            ProgressMessageHandler songDownloadHandler = new(new HttpClientHandler { AllowAutoRedirect = true }),
                albumDownloadHandler = new(new HttpClientHandler { AllowAutoRedirect = true });
            songDownloadHandler.HttpReceiveProgress += (_, args)
                => Debug.WriteLine($"{song.Name} progress: {args.ProgressPercentage}%");
            albumDownloadHandler.HttpReceiveProgress += (_, args)
                => Debug.WriteLine($"{song.Name} cover progress: {args.ProgressPercentage}%");
            HttpClient songClient = new(songDownloadHandler)
            {
                Timeout = TimeSpan.FromSeconds(1000)
            };
            HttpClient albumClient = new(albumDownloadHandler)
            {
                Timeout = TimeSpan.FromSeconds(1000)
            };

            // Downloading files
            _songTask = songClient.GetAsync(song.SongURL);
            _albumTask = albumClient.GetAsync(song.AlbumCoverURL);

            using var songStream = new FileStream(destinationSongFile, FileMode.Create);
            using var albumCoverStream = new FileStream(destinationAlbumCover, FileMode.Create);

            refSongTask = _songTask.Result.Content.CopyToAsync(songStream);
            refCoverTask = _albumTask.Result.Content.CopyToAsync(albumCoverStream);

            song.SongLocalPath = destinationSongFile;
            song.AlbumCoverLocalPath = destinationAlbumCover;

            return song;
        }

        public static async Task<Song> DownloadSongWithProgressBar(string destRootFolder, Song song)
        {
            if (DownloadDialog.IsBusy) return song;

            string destinationFolder = Path.Combine(destRootFolder, song.Id.ToString() ?? throw new InvalidOperationException()),
                destinationSongFile = Path.Combine(destinationFolder, song.Id + Path.GetExtension(song.SongURL)),
                destinationAlbumCover = Path.Combine(destinationFolder, song.Id + Path.GetExtension(song.AlbumCoverURL));
            if (Directory.Exists(destinationFolder)) Directory.Delete(destinationFolder, true);
            Directory.CreateDirectory(destinationFolder);

            DownloadDialog.Text = $"Downloading {song.Artists} - {song.Name}";
            DownloadDialog.DoWork += GetSongs_DoWork;

            DownloadDialog.Show();

            song.AlbumCoverLocalPath = destinationAlbumCover;
            song.SongLocalPath = destinationSongFile;

            do Thread.Sleep(500);
            while (_songWithDialogTask is null);
            _songWithDialogTask.Wait();

            do Thread.Sleep(500);
            while (_albumCoverWithDialogTask is null);
            _albumCoverWithDialogTask.Wait();

            return song;

            void GetSongs_DoWork(object? sender, DoWorkEventArgs e)
            {
                // Bug: I can't do the Download progress dialog report progress aaaaaaaaaaahhh
                ProgressMessageHandler songDownloadHandler = new(new HttpClientHandler { AllowAutoRedirect = true }),
                    albumDownloadHandler = new(new HttpClientHandler { AllowAutoRedirect = true });
                songDownloadHandler.HttpReceiveProgress += (_, args) =>
                {
                    _songProgress = args.ProgressPercentage;
                    Debug.WriteLine($"Song progress: {_songProgress}%");
                    DownloadDialog.ReportProgress(_songProgress, "Downloading", "Downloading song");
                };
                albumDownloadHandler.HttpReceiveProgress += (_, args) =>
                {
                    _albumProgress = args.ProgressPercentage;
                    Debug.WriteLine($"Album progress: {_albumProgress}%");
                };
                HttpClient songClient = new(songDownloadHandler)
                {
                    Timeout = TimeSpan.FromSeconds(1000)
                };
                HttpClient albumClient = new(albumDownloadHandler)
                {
                    Timeout = TimeSpan.FromSeconds(1000)
                };

                // TODO: Handle "System.InvalidOperationException: 'An invalid request URI was provided. Either the request URI must be an absolute URI or BaseAddress must be set.'"
                _songWithDialogTask = songClient.GetAsync(song.SongURL);

                /*while (!_songWithDialogTask.IsCompleted)
                    DownloadDialog.ReportProgress(_songProgress, "fdgdgdfg", "Downloading song");*/

                using var songStream = new FileStream(destinationSongFile, FileMode.Create);
                _songWithDialogTask.Result.Content.CopyToAsync(songStream);
                DownloadDialog.ReportProgress(0, "asdasd", "Downloading album cover");

                _albumCoverWithDialogTask = albumClient.GetAsync(song.AlbumCoverURL);

                //while (!_albumCoverWithDialogTask.IsCompleted)
                //    DownloadDialog.ReportProgress(_albumProgress, DownloadDialog.Text, "Downloading song");

                using var albumCoverStream = new FileStream(destinationAlbumCover, FileMode.Create);
                _albumCoverWithDialogTask.Result.Content.CopyToAsync(albumCoverStream);

                DownloadDialog.ReportProgress(100, "Finished", "Song downloaded");
                DownloadDialog.Dispose();
            }
        }

        public static void StopAllDownloads()
        {
            if (_songWithDialogTask != null)
                _songWithDialogTask.Wait();
            if (_albumCoverWithDialogTask != null)
                _albumCoverWithDialogTask.Wait();
            if (_songTask != null)
                _songTask.Wait();
            if (_albumTask != null)
                _albumTask.Wait();
        }
    }
}