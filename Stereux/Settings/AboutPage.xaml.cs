using System.Windows;
using System.Windows.Controls;

namespace Stereux.Settings
{
    /// <summary>
    /// Lógica de interacción para AboutPage.xaml
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var link = (sender as Button)!.CommandParameter.ToString() switch
            {
                "Stereux" => "https://github.com/LuisAlfredo92/Stereux",
                "GitHub" => "https://github.com/LuisAlfredo92/",
                "Donate" => "https://paypal.me/LuisAlfredo92",
                "Microsoft.AspNet.WebApi.Client" => "https://www.asp.net/web-api",
                "Microsoft author" => "https://www.nuget.org/profiles/Microsoft",
                "Microsoft license" => "https://www.nuget.org/packages/Microsoft.AspNet.WebApi.Client/5.2.9/license",
                "Microsoft.EntityFrameworkCore" => "https://docs.microsoft.com/ef/core/",
                "MIT license" => "https://licenses.nuget.org/MIT",
                "Net.Codecrete.QrCodeGenerator" => "https://github.com/manuelbl/QrCodeGenerator",
                "Manuel Bleichenbacher" => "https://github.com/manuelbl",
                "Octokit" => "https://github.com/octokit/octokit.net",
                "Octokit author" => "https://github.com/octokit",
                "Ookii.Dialogs.Wpf" => "https://github.com/ookii-dialogs/ookii-dialogs-wpf",
                "Ookii Dialogs Contributors" => "https://github.com/ookii-dialogs",
                "BSD-3-Clause license" => "https://licenses.nuget.org/BSD-3-Clause",
                "ScrapySharp" => "https://github.com/rflechner/ScrapySharp",
                "Romain Flechner" => "https://github.com/rflechner",
                "System.Data.SqlClient" => "https://github.com/dotnet/corefx",
                "TagLibSharp" => "https://github.com/mono/taglib-sharp",
                "TagLibSharp author" => "https://github.com/mono",
                "LGPL-2.1-only license" => "https://licenses.nuget.org/LGPL-2.1-only",
                _ => "https://github.com/LuisAlfredo92/Stereux"
            };
            System.Diagnostics.Process.Start(link);
        }
    }
}