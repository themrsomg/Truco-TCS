using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.Audio;

namespace TrucoClient.Views
{
    public partial class CreditsPage : Page
    {
        private const string EASTER_EGG_FILE_NAME = "NobodyBeatsTheHammer.exe";
        private const string RESOURCES_FOLDER = "Resources";

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
            string fullPath = System.IO.Path.Combine(currentDirectory, RESOURCES_FOLDER, EASTER_EGG_FILE_NAME);

            try
            {
                if (!File.Exists(fullPath))
                {
                    CustomMessageBox.Show(Lang.EasterEggTextFileNotFound, Lang.GlobalTextRuntimeError, 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Process.Start(fullPath);
                if (this.NavigationService != null)
                {
                    this.NavigationService.Navigate(new SettingsPage());
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                CustomMessageBox.Show(Lang.EasterEggTextExecuteError, Lang.GlobalTextCriticalError, 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, Lang.GlobalTextRuntimeError, 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
