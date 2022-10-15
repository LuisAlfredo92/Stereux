using System.Windows;

namespace Stereux
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            StartupUri = Stereux.Properties.Settings.Default.FirstTimeOpening
                ? new Uri("WelcomeWindow.xaml", UriKind.Relative)
                : new Uri("Player.xaml", UriKind.Relative);
        }
    }
}