using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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