using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Properties.Langs;

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
            string fullPath = Path.Combine(currentDirectory, RESOURCES_FOLDER, EASTER_EGG_FILE_NAME);

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
            catch (System.ComponentModel.Win32Exception ex)
            {
                ClientException.HandleError(ex, nameof(ClickEgg));
                CustomMessageBox.Show(Lang.EasterEggTextExecuteError, Lang.GlobalTextCriticalError, 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(ClickEgg));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, Lang.GlobalTextRuntimeError, 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
