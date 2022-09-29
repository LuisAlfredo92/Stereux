using Ookii.Dialogs.Wpf;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Stereux.Settings
{
    /// <summary>
    /// Lógica de interacción para SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            DataPathTextBox.Text = Properties.Settings.Default.DataPath;
        }

        private void DefaultFolderBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DataPath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Stereux"
                );
            Properties.Settings.Default.Save();
            DataPathTextBox.Text = Properties.Settings.Default.DataPath;
        }

        private void SelectFolderBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true
            };

            var wind = Application.Current.Windows.OfType<SettingsWindow>().First();
            if (!(bool)dialog.ShowDialog(wind)) return;

            Properties.Settings.Default.DataPath = Path.Combine(dialog.SelectedPath, "Stereux");
            Properties.Settings.Default.Save();
            DataPathTextBox.Text = Properties.Settings.Default.DataPath;
        }
    }
}