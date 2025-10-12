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
using TrucoPrueba1.Properties.Langs;

namespace TrucoPrueba1
{
    /// <summary>
    /// Lógica de interacción para InitialWindows.xaml
    /// </summary>
    public partial class InitialWindows : Window
    {
        public InitialWindows()
        {
            InitializeComponent();
            MainFrame.Navigate(new StartPage());
            MainFrame.Navigated += MainFrame_Navigated;
            string trackPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "Songs",
                "music_in_menus.mp3"
            );
            MusicManager.Play(trackPath);
            MusicManager.Volume = 0.3;
        }
        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Page page = MainFrame.Content as Page;

            if (page != null)
            {
                if (!string.IsNullOrEmpty(page.Title))
                {
                    this.Title = page.Title;
                }
                if (!double.IsNaN(page.Height) && !double.IsNaN(page.Width))
                {
                    this.Height = page.Height;
                    this.Width = page.Width;
                }
            }
        }
    }
}
