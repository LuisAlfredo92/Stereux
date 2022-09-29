using System.IO;
using System.Windows;
using Stereux.Settings;

namespace Stereux
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : Window
    {
        public Player()
        {
            InitializeComponent();

            if (Properties.Settings.Default.DataPath.Length < 1)
            {
                Properties.Settings.Default.DataPath =
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Stereux"
                        );
                Properties.Settings.Default.Save();
            }
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().Show();
        }
    }
}