using System;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Localization;
using TrucoClient.Helpers.Session;
using TrucoClient.Properties;
using TrucoClient.Properties.Langs;


namespace TrucoClient.Views
{
    public partial class StartPage : Page
    {
        private const string MESSAGE_ERROR = "Error";
        public StartPage()
        {
            InitializeComponent();
            SessionManager.Clear();
            ApplyDefaultLanguageAndAudio();

            MusicInitializer.InitializeMenuMusic();
        }

        private static void ApplyDefaultLanguageAndAudio()
        {
            try
            {
                const string DEFAULT_LANG = "es-MX";

                if (Settings.Default.languageCode != DEFAULT_LANG)
                {
                    Settings.Default.languageCode = DEFAULT_LANG;
                    Settings.Default.Save();

                    LanguageManager.ChangeLanguage(DEFAULT_LANG);
                }

                if (MusicManager.IsMuted)
                {
                    MusicManager.ToggleMute();
                }
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickLogIn(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LogInPage());
        }

        private void ClickPlayAsGuest(object sender, RoutedEventArgs e)
        {
            string guestName = "Guest_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            SessionManager.CurrentUsername = guestName;
            this.NavigationService.Navigate(new MainPage());
        }

        private void ClickSingUp(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new NewUserPage());

        }

        private void ClickExit(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
    }
}
