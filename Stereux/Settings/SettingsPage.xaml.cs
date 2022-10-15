using Ookii.Dialogs.Wpf;
using System.IO;
using System.Windows;
using Connections.SongsDSTableAdapters;
using Downloader;

namespace Stereux.Settings
{
    /// <summary>
    /// Lógica de interacción para SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage
    {
        private string _stereuxVersion;

        /* TODO: Add option to choose if move folder to new path or
         * delete it and create it again
         */

        public SettingsPage()
        {
            InitializeComponent();

            var currentPath = Properties.Settings.Default.DataPath;
            DataPathTextBox.Text = currentPath;
            CalculateFolderSize();

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            _stereuxVersion = fvi.FileVersion!;
            UpdateStereuxTextBlock.Text = $"Current version: {_stereuxVersion}";
        }

        private void DefaultFolderBtn_OnClick(object sender, RoutedEventArgs e)
        {
            // Delete existing folder
            if (Directory.Exists(Properties.Settings.Default.DataPath))
                Directory.Delete(Properties.Settings.Default.DataPath, true);
            CalculateFolderSize();
            new SongsTableAdapter().DeleteLocalFiles();

            // Set default folder as new folder
            Properties.Settings.Default.DataPath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Stereux"
                );
            Properties.Settings.Default.Save();
            DataPathTextBox.Text = Properties.Settings.Default.DataPath;
            Directory.CreateDirectory(Properties.Settings.Default.DataPath);
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

            // Delete existing folder
            if (Directory.Exists(Properties.Settings.Default.DataPath))
                Directory.Delete(Properties.Settings.Default.DataPath, true);
            CalculateFolderSize();
            new SongsTableAdapter().DeleteLocalFiles();

            // Set selected folder as new folder
            Properties.Settings.Default.DataPath = Path.Combine(dialog.SelectedPath, "Stereux");
            Properties.Settings.Default.Save();
            DataPathTextBox.Text = Properties.Settings.Default.DataPath;
            Directory.CreateDirectory(Properties.Settings.Default.DataPath);
        }

        private void DeleteStereuxFolderBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var path = Properties.Settings.Default.DataPath;
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            CalculateFolderSize();
            new SongsTableAdapter().DeleteLocalFiles();
            Directory.CreateDirectory(path);
        }

        private async void CalculateFolderSize()
        {
            if (!Directory.Exists(Properties.Settings.Default.DataPath))
                FolderSizeLabel.Content = AddSuffixToBytes(0);
            else
            {
                DirectoryInfo di = new(Properties.Settings.Default.DataPath);
                FolderSizeLabel.Content =
                    AddSuffixToBytes(
                        di.EnumerateFiles("*", SearchOption.AllDirectories)
                            .Sum(fi => fi.Length)
                        );
            }

            string AddSuffixToBytes(long value)
            {
                string[] sizeSuffixes = { "Bytes", "KB", "MB", "GB" };

                byte i = 0;
                var dValue = (decimal)value;
                while (Math.Round(dValue / 1024) >= 1)
                {
                    dValue /= 1024;
                    i++;
                }

                return $"{dValue:n1} {sizeSuffixes[i]}";
            }
        }

        private void ShowWelcomeWindowBtn_OnClick(object sender, RoutedEventArgs e)
            => new WelcomeWindow().Show();

        private void CheckUpdatesBtn_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: Uncomment this to enable the updater
            //if (Updater.CheckUpdates(stereuxVersion).Result)
            //{
            //    var boxResult = MessageBox.Show("There's a new version available. Do you want to download it?", "New version available",
            //        MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
            //    if (boxResult == MessageBoxResult.Yes)
            //    {
            //        System.Diagnostics.Process.Start("https://github.com/LuisAlfredo92/Stereux/Releases/latest");
            //        Close();
            //    }
            //}
            //else
            //    MessageBox.Show("You have the latest version of Stereux", "No new versions", MessageBoxButton.OK,
            //        MessageBoxImage.Information);
        }
    }
}