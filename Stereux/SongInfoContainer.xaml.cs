using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Connections.Models;
using Connections.SongsDSTableAdapters;

namespace Stereux
{
    /// <summary>
    /// Lógica de interacción para SongInfoContainer.xaml
    /// </summary>
    public partial class SongInfoContainer : Page
    {
        private int _infoIndex;

        // For each array, the order is 1. Stereux, 2. Source, 3. Song
        private string[] _titles = { "Download Stereux", "Music provided by:", "Get more info" };

        private string[] _subtitles = { "from here!", string.Empty, "about this song" };

        private DrawingImage[] _qrCodes =
        {
            QrGenerator.Generator.GenerateQr("https://github.com/LuisAlfredo92/Stereux"),
            QrGenerator.Generator.GenerateQr("https://github.com/LuisAlfredo92/Stereux"),
            QrGenerator.Generator.GenerateQr("https://github.com/LuisAlfredo92/Stereux")
        };

        public SongInfoContainer()
        {
            _infoIndex = 0;
            InitializeComponent();
            titleLabel.Text = _titles[_infoIndex];
            subtitleLabel.Text = _subtitles[_infoIndex];
            qrCodeImage.Source = _qrCodes[_infoIndex];
            DispatcherTimer timer = new()
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            timer.Tick += ChangeView;
            timer.Start();
        }

        private void ChangeVars(Sources source, string songUrl)
        {
            _subtitles[1] = source switch
            {
                Sources.Ncs => "No Copyright Sounds",
                _ => "Unknown"
            };
            _qrCodes[1] = source switch
            {
                Sources.Ncs => QrGenerator.Generator.GenerateQr("https://ncs.io"),
                _ => QrGenerator.Generator.GenerateQr("")
            };
            _qrCodes[2] = QrGenerator.Generator.GenerateQr(songUrl);
        }

        private void ChangeView(object? sender, EventArgs e)
        {
            _infoIndex = _infoIndex < 2 ? _infoIndex + 1 : 0;
            titleLabel.Text = _titles[_infoIndex];
            subtitleLabel.Text = _subtitles[_infoIndex];
            qrCodeImage.Source = _qrCodes[_infoIndex];
        }
    }
}