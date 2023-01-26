using System.Diagnostics;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Reflection;
using System.Windows;
using Connections.SongsDSTableAdapters;
using Downloader;
using System.Security.Policy;

namespace Stereux.Settings
{
    /// <summary>
    /// Page in the Settings window where the user can modify some
    /// settings like DataPath or Stereux size
    /// </summary>
    public partial class SettingsPage
    {
        /// <summary>
        /// The stereux version.
        /// </summary>
        private readonly string _stereuxVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
            var currentPath = Properties.Settings.Default.DataPath;
            DataPathTextBox.Text = currentPath;

            var assembly = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Stereux.exe");
            var fvi = FileVersionInfo.GetVersionInfo(assembly);
            _stereuxVersion = fvi.FileVersion!;
            UpdateStereuxTextBlock.Text = $"Current version: {_stereuxVersion}";

            CalculateFolderSize();
        }

        /// <summary>
        /// Establishes the DataPath (directory where everything will be saved)
        /// to the default %APPDATA%\Stereux folder
        /// </summary>
        private void DefaultFolderBtn_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: Add option to choose if move folder to new path or delete it and create it again
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

        /// <summary>
        /// Shows a window to select a new DataPath folder (Where everything will be saved)
        /// </summary>
        private void SelectFolderBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true
            };

            var wind = Application.Current.Windows.OfType<SettingsWindow>().First();
            if (!dialog.ShowDialog(wind)!.Value) return;

            // Delete existing folder
            // TODO: Add option to choose if move folder to new path or delete it and create it again
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

        /// <summary>
        /// Deletes the stereux folder (where songs and covers are saved)
        /// </summary>
        private void DeleteStereuxFolderBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var path = Properties.Settings.Default.DataPath;
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            CalculateFolderSize();
            new SongsTableAdapter().DeleteLocalFiles();
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Calculates the Stereux folder size.
        /// </summary>
        private void CalculateFolderSize()
        {
            if (Properties.Settings.Default.DataPath.Length < 1)
                return;
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

            static string AddSuffixToBytes(long value)
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

        /// <summary>
        /// Shows the welcome window on click.
        /// </summary>
        private void ShowWelcomeWindowBtn_OnClick(object sender, RoutedEventArgs e)
            => new WelcomeWindow(true).Show();

        /// <summary>
        /// Checks updates click.
        /// </summary>
        private void CheckUpdatesBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (Updater.CheckUpdates(_stereuxVersion))
            {
                var boxResult = MessageBox.Show("There's a new version available. Do you want to download it?", "New version available",
                    MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
                if (boxResult != MessageBoxResult.Yes) return;

                // Thanks to https://stackoverflow.com/a/43232486/11756870
                /* I removed the OSX and Linux part since this program will
                 * be Windows exclusive
                 */
                var url = "https://github.com/LuisAlfredo92/Stereux/releases/latest";
                try
                {
                    Process.Start(url);
                }
                catch
                {
                    // hack because of this: https://github.com/dotnet/corefx/issues/10361
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
            }
            else
                MessageBox.Show("You have the latest version of Stereux", "No new versions", MessageBoxButton.OK,
                    MessageBoxImage.Information);
        }
    }
}