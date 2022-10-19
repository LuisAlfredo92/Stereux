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
using Connections.SongsDSTableAdapters;

namespace Downloader;

/// <summary>
/// The class that will download Song file (mp3) and its album cover (png or jpg)
/// asynchronously, it can show a progress dialog when downloading.
/// </summary>
public class Downloader
{
    /// <summary>
    /// The download dialog.
    /// </summary>
    private static readonly ProgressDialog DownloadDialog = new()
    {
        WindowTitle = "Downloading",
        Text = "Downloading song",
        Description = "Downloading song",
        ShowCancelButton = false,
        ShowTimeRemaining = true
    };

    /// <summary>
    /// Tasks used to download songs and covers. There's one for download with and without Progress dialog
    /// </summary>
    private static Task<HttpResponseMessage>? _songWithDialogTask, _albumCoverWithDialogTask, _songTask, _albumTask;

    /// <summary>
    /// The value of the download progress for song and album cover
    /// </summary>
    private static int _songProgress, _albumProgress;

    /// <summary>
    /// Downloads a song without showing a progress dialog, only writing progress in Debug.
    /// </summary>
    /// <param name="destRootFolder">The destination folder, it's usually the DataPath in Settings.</param>
    /// <param name="song">The song to be downloaded.</param>
    /// <param name="refSongTask">The Task that will end when a song is downloaded. Used to handle multiple downloads out of this class</param>
    /// <param name="refCoverTask">The Task that will end when an album cover is downloaded. Used to handle multiple downloads out of this class</param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns>The same received song but with the SongLocalPath and AlbumCoverLocalPath changed so Player can read their files</returns>
    public static Song DownloadSong(string destRootFolder, Song song, out Task refSongTask, out Task refCoverTask)
    {
        string destinationFolder = Path.Combine(destRootFolder, song.Id.ToString() ?? throw new ArgumentException("Song id can't be null")),
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

        new SongsTableAdapter().SongDownloaded(destinationAlbumCover, destinationSongFile, song.Id);

        return song;
    }

    /// <summary>
    /// Downloads a song showing a progress dialog and writing progress in Debug.
    /// </summary>
    /// <param name="destRootFolder">The destination folder, it's usually the DataPath in Settings.</param>
    /// <param name="song">he song to be downloaded.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns><![CDATA[Task<Song>]]> that represents if the song has been downloaded with the respective paths to the files</returns>
    public static async Task<Song> DownloadSongWithProgressBar(string destRootFolder, Song song)
    {
        if (DownloadDialog.IsBusy) return song;

        string destinationFolder = Path.Combine(destRootFolder, song.Id.ToString() ?? throw new ArgumentException("Song id can't be null")),
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

        new SongsTableAdapter().SongDownloaded(destinationAlbumCover, destinationSongFile, song.Id);

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

    /// <summary>
    /// Waits for every download to be stopped
    /// </summary>
    public static void StopAllDownloads()
    {
        _songWithDialogTask?.Wait();
        _albumCoverWithDialogTask?.Wait();
        _songTask?.Wait();
        _albumTask?.Wait();
    }
}