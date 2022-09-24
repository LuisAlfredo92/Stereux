using System.Windows.Controls;
using Connections.SongsDSTableAdapters;

namespace Stereux
{
    /// <summary>
    /// Lógica de interacción para SongInfoContainer.xaml
    /// </summary>
    public partial class SongInfoContainer : Page
    {
        public SongInfoContainer()
        {
            InitializeComponent();
            var asd = new SongsTableAdapter().GetData();
        }
    }
}