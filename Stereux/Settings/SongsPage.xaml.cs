using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Connections.Controllers;
using Connections.Models;
using Connections.SongsDSTableAdapters;

namespace Stereux.Settings
{
    /// <summary>
    /// Lógica de interacción para SongsPage.xaml
    /// </summary>
    public partial class SongsPage : Page
    {
        private SongsTableAdapter _songsTable;

        public SongsPage()
        {
            InitializeComponent();
            _songsTable = new SongsTableAdapter();
            SongsDataGrid.ItemsSource = _songsTable.GetData();
        }

        private async void GetSongsBtn_OnClick(object sender, RoutedEventArgs e)
        {
            StateLabel.Content = "Working...";
            StateLabel.UpdateLayout();
            List<Song> songs = new();
            var enabledSources = new SourcesTableAdapter().GetEnabledSources();
            foreach (var source in enabledSources)
            {
                //TODO: Learn how to do this with Threads
                var songsObtained = source.Id switch
                {
                    1 => await new Ncs().GetSongs(),
                    _ => null
                };
                if (songsObtained is not null)
                    songs.AddRange(songsObtained);
            }

            if (songs.Count != _songsTable.InsertSong(songs))
            {
                //TODO: Handle error
                Debug.WriteLine("Not every song was added");
            }

            SongsDataGrid.ItemsSource = _songsTable.GetData();
            StateLabel.Content = "Done!";
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

        private void GenerateQrBtn_OnClick(object sender, RoutedEventArgs e)
        {
            //TODO: Generate Qr
        }
    }
}