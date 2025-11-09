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
            MainFrame.Navigate(new Views.SplashPage());
            MainFrame.Navigated += MainFrameNavigated;
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
