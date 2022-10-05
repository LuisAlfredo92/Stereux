using System.Data;
using System.Windows;
using System.Windows.Controls;
using SourcesTableAdapter = Connections.SongsDSTableAdapters.SourcesTableAdapter;

namespace Stereux.Settings
{
    /// <summary>
    /// Lógica de interacción para SourcesPage.xaml
    /// </summary>
    public partial class SourcesPage : Page
    {
        private readonly SourcesTableAdapter _table;

        public SourcesPage()
        {
            _table = new SourcesTableAdapter();
            InitializeComponent();
            SourcesDataGrid.ItemsSource = _table.GetData();
        }

        private void OnEnabledChanged(object sender, RoutedEventArgs e)
        {
            var parameter = (sender as CheckBox)?.CommandParameter as DataRowView;
            var id = parameter!.Row["Id"] as int?;
            var currentCheckedValue = (sender as CheckBox)?.IsChecked;

            _table.SetEnabled(currentCheckedValue, id);
        }
    }
}