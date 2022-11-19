using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Connections.Controllers;
using Connections.Models;
using Connections.SongsDSTableAdapters;
using Ookii.Dialogs.Wpf;

namespace Stereux.Introduction
{
    /// <summary>
    /// Page that will be showed in the Introduction window,
    /// where the user can download songs from the internet
    /// </summary>
    public partial class Page2
    {
        /// <summary>
        /// The songs table.
        /// </summary>
        private readonly SongsTableAdapter _songsTable;

        /// <summary>
        /// The sources table.
        /// </summary>
        private readonly SourcesTableAdapter _sourcesTable;

        /// <summary>
        /// The progress dialog that will report progress of all the download process.
        /// </summary>
        private readonly ProgressDialog _entireProgressDialog = new()
        {
            WindowTitle = "Getting songs info",
            Text = "Getting info of the songs from",
            Description = "Processing...",
            ShowCancelButton = false
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Page2"/> class.
        /// </summary>
        public Page2()
        {
            InitializeComponent();
            try
            {
                _songsTable = new SongsTableAdapter();
                _sourcesTable = new SourcesTableAdapter();
                SongsDataGrid.ItemsSource = _songsTable.GetData();
                SourcesDataGrid.ItemsSource = _sourcesTable.GetData();
            }
            catch (Exception e)
            {
                File.WriteAllText("Error.txt", e.Message);
                throw;
            }
        }

        /// <summary>
        /// Changes the Enabled state on Database when it's changed on the Grid view
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnEnabledChanged(object sender, RoutedEventArgs e)
        {
            var parameter = (sender as CheckBox)?.CommandParameter as DataRowView;
            var id = parameter!.Row["Id"] as int?;
            var currentCheckedValue = (sender as CheckBox)?.IsChecked;

            _sourcesTable.SetEnabled(currentCheckedValue, id);
        }

        /// <summary>
        /// Shows a progress dialog and calls the method that will get
        /// ALL the songs from ALL the enabled sources
        /// </summary>
        private void GetSongsBtn_OnClick(object sender, RoutedEventArgs e)
        {
            GetSongsBtn.IsEnabled = false;
            if (_entireProgressDialog.IsBusy)
                MessageBox.Show("Songs are already being obtained", "Work in progress");
            else
            {
                _entireProgressDialog.DoWork += GetSongsFromAllSources_DoWork;
                _entireProgressDialog.RunWorkerCompleted += (_, _) => SongsDataGrid.ItemsSource = _songsTable.GetData();
                _entireProgressDialog.Show();
            }
        }

        /// <summary>
        /// Deletes one song from database.
        /// </summary>
        private void DeleteSongBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var id = ((sender as Button)!.CommandParameter as int?) ?? -1;
            _songsTable.DeleteSong(id);
        }

        /// <summary>
        /// Downloads one song from the Internet.
        /// </summary>
        private void DownloadBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var id = ((sender as Button)!.CommandParameter as int?)!;
            Song? song = new(_songsTable.GetSong(id));
            song = Downloader.Downloader.DownloadSongWithProgressBar(Properties.Settings.Default.DataPath, song).Result;
            SongsDataGrid.ItemsSource = _songsTable.GetData();
        }

        /// <summary>
        /// Gets ALL the songs from ALL the enabled sources.
        /// It must be called carefully since it's a heavy
        /// and slow function
        /// </summary>
        private void GetSongsFromAllSources_DoWork(object? sender, DoWorkEventArgs e)
        {
            List<Song> songs = new();
            var enabledSources = new SourcesTableAdapter().GetEnabledSources();
            if (enabledSources.Count == 0)
            {
                MessageBox.Show("You didn't select any source, go to Sources page and select at least one",
                    "No selected sources",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                _entireProgressDialog.ReportProgress(100,
                    "Getting songs",
                    $"Progress: {100}%");
                return;
            }

            byte progress = 0;
            foreach (var source in enabledSources)
            {
                // Report progress to progress dialog
                _entireProgressDialog.ReportProgress(progress,
                    $"Getting songs from {source.Name}",
                    $"Progress: {progress}%");

                List<Song>? songsObtained;
                try
                {
                    songsObtained = source.Id switch
                    {
                        1 => new Ncs().GetSongs(),
                        _ => null
                    };
                }
                catch (Exception)
                {
                    songsObtained = null;
                }
                if (songsObtained is not null)
                    songs.AddRange(songsObtained);

                // Checks if user has cancelled
                if (_entireProgressDialog.CancellationPending)
                    return;
                progress += (byte)(enabledSources.Count / 100);
            }

            _entireProgressDialog.ReportProgress(progress,
                "Getting songs finished",
                $"Progress: {progress}%");

            if (songs.Count != _songsTable.InsertSong(songs))
            {
                //TODO: Handle error
                Debug.WriteLine("Not every song was added");
            }
            _entireProgressDialog.Dispose();
        }
    }
}