using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Stereux
{
    /// <summary>
    /// Lógica de interacción para Introduction.xaml
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        private byte _page = 1;

        public WelcomeWindow()
        {
            InitializeComponent();
        }

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            _page--;
            PageLabel.Content = $"{_page} / 3";
            ContainerFrame.Source = new Uri($"Introduction/Page{_page}.xaml", UriKind.Relative);
            if (_page < 2)
                PreviousButton.IsEnabled = false;
            if (_page < 3)
                NextButton.Content = "Next ↪";
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_page < 3)
            {
                _page++;
                PageLabel.Content = $"{_page} / 3";
                PreviousButton.IsEnabled = true;
                ContainerFrame.Source = new Uri($"Introduction/Page{_page}.xaml", UriKind.Relative);
                if (_page == 3)
                    NextButton.Content = "Finish";
            }
            else
            {
                Properties.Settings.Default.FirstTimeOpening = false;
                Properties.Settings.Default.Save();
                new Player().Show();
                Close();
            }
        }
    }
}