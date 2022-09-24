using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using Connections;
using Connections.SongsDSTableAdapters;

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