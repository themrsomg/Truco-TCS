using Microsoft.ServiceFabric.Services.Communication;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Localization;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Properties;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class SettingsPage : Page
    {
        private const String VOLUME_ICON_MUTED_PATH = "/Resources/Logos/logo_muted.png";
        private const String VOLUME_ICON_VOLUME_PATH = "/Resources/Logos/logo_volume.png";
        private const String MESSAGE_ERROR = "Error";

        public SettingsPage()
        {
            InitializeComponent();

            MusicInitializer.InitializeMenuMusic();
            UpdateVolumeIcon();

            SetInitialLanguage();
        }

        public SettingsPage(int selectedLanguageIndex)
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
            UpdateVolumeIcon();

            cbLanguages.SelectedIndex = selectedLanguageIndex;
        }

        private void UpdateVolumeIcon()
        {
            string iconPath = MusicManager.IsMuted
                ? VOLUME_ICON_MUTED_PATH
                : VOLUME_ICON_VOLUME_PATH;

            imgVolumeIcon.Source = new BitmapImage(new Uri(iconPath, UriKind.Relative));
        }

        private void ClickVolume(object sender, RoutedEventArgs e)
        {
            MusicManager.ToggleMute();
            UpdateVolumeIcon();
        }

        private void ClickAbout(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new AboutPage());
        }

        private void ClickCredits(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CreditsPage());
        }

        private async void ClickSave(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsMusicMuted = MusicManager.IsMuted;
            Settings.Default.Save();

            await SavePreferencesToServer();

            this.NavigationService.Navigate(new MainPage());
        }

        private async Task SavePreferencesToServer()
        {
            if (SessionManager.CurrentUserData == null ||
               (SessionManager.CurrentUsername != null && SessionManager.CurrentUsername.StartsWith("Guest_")))
            {
                return;
            }

            try
            {
                SessionManager.CurrentUserData.LanguageCode = Settings.Default.languageCode;
                SessionManager.CurrentUserData.IsMusicMuted = MusicManager.IsMuted;

                await ClientManager.UserClient.SaveUserProfileAsync(SessionManager.CurrentUserData);
            }
            catch (ServiceException ex)
            {
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Lang.ExceptionTextArgument, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SetInitialLanguage()
        {
            string currentLang = Settings.Default.languageCode;
            
            if (currentLang == "es-MX")
            {
                cbLanguages.SelectedIndex = 1;
            }
            else
            {
                cbLanguages.SelectedIndex = 0;
            }
        }

        private void LanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbLanguages.SelectedItem == null)
            {
                return;
            }

            int selectedIndex = cbLanguages.SelectedIndex;
            string newLang = (selectedIndex == 0) ? "en-US" : "es-MX";

            if (Settings.Default.languageCode == newLang)
            {
                return;
            }

            LanguageManager.ChangeLanguage(newLang);

            this.NavigationService.Navigate(new SettingsPage(selectedIndex));
        }

        private void ClickCards(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CardsFirstPage());
        }
    }
}
