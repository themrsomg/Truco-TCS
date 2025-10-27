using System;
using System.Windows;
using System.Windows.Controls;

namespace TrucoClient
{
    public partial class InitialWindows : Window
    {
        public InitialWindows()
        {
            InitializeComponent();
            MainFrame.Navigate(new StartPage());
            MainFrame.Navigated += MainFrameNavigated;
            string trackPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "Songs",
                "music_in_menus.mp3"
            );
            MusicManager.Play(trackPath);
            MusicManager.Volume = 0.3;
        }
        private void MainFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
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
