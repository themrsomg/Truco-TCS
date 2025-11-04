using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using TrucoClient.Properties.Langs;

namespace TrucoClient
{
    public partial class CreditsPage : Page
    {
        public CreditsPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }
        private void ClickEgg(object sender, RoutedEventArgs e)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = System.IO.Path.Combine(currentDirectory, "Resources", "NobodyBeatsTheHammer.exe");

            try
            {
                if (!File.Exists(fullPath))
                {
                    MessageBox.Show(string.Format(Lang.EasterEggTextFileNotFound, fullPath), Lang.GlobalTextRuntimeError, 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Process.Start(fullPath);
                if (this.NavigationService != null)
                {
                    this.NavigationService.Navigate(new SettingsPage());
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show(string.Format(Lang.EasterEggTextExecuteError, ex.Message), Lang.GlobalTextCriticalError, 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", Lang.GlobalTextRuntimeError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
