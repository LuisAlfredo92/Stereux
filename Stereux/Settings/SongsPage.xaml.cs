using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Connections.Controllers;
using Connections.Models;
using Connections.SongsDSTableAdapters;
using Ookii.Dialogs.Wpf;

namespace Stereux.Settings
{
    /// <summary>
    /// Lógica de interacción para SongsPage.xaml
    /// </summary>
    public partial class SongsPage : Page
    {
        private SongsTableAdapter _songsTable;

        private ProgressDialog _entireProgressDialog = new()
        {
            WindowTitle = "Getting songs info",
            Text = "Getting info of the songs from",
            Description = "Processing...",
            ShowCancelButton = false
        };

        public SongsPage()
        {
            InitializeComponent();
            _songsTable = new SongsTableAdapter();
            SongsDataGrid.ItemsSource = _songsTable.GetData();
        }

        private async void GetSongsBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_entireProgressDialog.IsBusy)
                MessageBox.Show("Songs are already being obtained", "Work in progress");
            else
            {
                _entireProgressDialog.DoWork += GetSongsFromAllSources_DoWork;
                _entireProgressDialog.RunWorkerCompleted += UpdateTable;
                _entireProgressDialog.Show();
            }
        }

        private void ClearSongsBtn_OnClick(object sender, RoutedEventArgs e)
        {
            _songsTable.TruncateTable();
            SongsDataGrid.ItemsSource = _songsTable.GetData();
        }

        private void DeleteSongBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var id = ((sender as Button)!.CommandParameter as int?) ?? -1;
            _songsTable.DeleteSong(id);
        }

        private void DownloadBtn_OnClick(object sender, RoutedEventArgs e)
        {
            //TODO: Download song
        }

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

        private void UpdateTable(object? sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs) => SongsDataGrid.ItemsSource = _songsTable.GetData();
    }
}