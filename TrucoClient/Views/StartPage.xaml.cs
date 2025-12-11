using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Localization;
using TrucoClient.Helpers.Session;
using TrucoClient.Properties;
using TrucoClient.Properties.Langs;


namespace TrucoClient.Views
{
    public partial class StartPage : Page
    {
        private const string MESSAGE_ERROR = "Error";
        private const string DEFAULT_LANG = "es-MX";
        private const string GUEST_NAME = "Guest_";


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
            catch (CultureNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(ApplyDefaultLanguageAndAudio));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(ApplyDefaultLanguageAndAudio));
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
            string guestName = GUEST_NAME + Guid.NewGuid().ToString("N").Substring(0, 6);
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
