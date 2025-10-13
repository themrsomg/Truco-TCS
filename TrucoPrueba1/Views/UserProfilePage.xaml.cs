using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrucoPrueba1.TrucoServer;
using TrucoPrueba1.Properties.Langs;

namespace TrucoPrueba1
{
    public static class SessionManager
    {
        private static readonly TrucoCallbackHandler callbackHandler = new TrucoCallbackHandler();
        private static readonly InstanceContext userContext = new InstanceContext(callbackHandler);
        private static TrucoUserServiceClient _userClient;

        public static TrucoUserServiceClient UserClient
        {
            get
            {
                if (_userClient == null || _userClient.State == CommunicationState.Faulted)
                {
                    _userClient = new TrucoUserServiceClient(userContext, "NetTcpBinding_ITrucoUserService");
                }
                return _userClient;
            }
        }

        public static string CurrentUsername { get; set; } = "UsuarioActual";
    }

    public partial class UserProfilePage : Page
    {
        private UserProfileData _currentUserData;

        private readonly List<string> AvailableAvatars = new List<string>
        {
            "avatar_default", "avatar_acewolf", "avatar_cthulu", "avatar_elaoctopus",
            "avatar_gears", "avatar_hydranoid", "avatar_maverickpanter", "avatar_redgi",
            "avatar_rose", "avatar_shark", "avatar_valkirya",
        };

        private const int MAX_CHANGES = 2;
        private string _originalUsername;

        public UserProfilePage()
        {
            InitializeComponent();
            LoadUserProfile();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void LoadUserProfile()
        {
            if (string.IsNullOrWhiteSpace(SessionManager.CurrentUsername) ||
                SessionManager.CurrentUsername == "UsuarioActual")
            {
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                var client = SessionManager.UserClient;

                _currentUserData = await client.GetUserProfileAsync(SessionManager.CurrentUsername);

                if (_currentUserData == null) return;

                this.DataContext = _currentUserData;
                _originalUsername = _currentUserData.Username;

                txtUsername.Text = _currentUserData.Username;
                txtEmail.Text = _currentUserData.Email; 
                txtFacebookLink.Text = _currentUserData.FacebookHandle;
                txtXLink.Text = _currentUserData.XHandle;
                txtInstagramLink.Text = _currentUserData.InstagramHandle;

                UpdateUsernameWarning(_currentUserData.NameChangeCount);
                UpdateSocialMediaLinks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async void ClickSave(object sender, RoutedEventArgs e)
        {
            if (_currentUserData == null) return;

            string newUsername = txtUsername.Text.Trim();
            if (string.IsNullOrWhiteSpace(newUsername)) return;

            Button saveButton = sender as Button;
            saveButton.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                bool usernameChanged = newUsername != _originalUsername;

                if (usernameChanged && _currentUserData.NameChangeCount >= MAX_CHANGES)
                {
                    MessageBox.Show(Lang.UserProfileTextTwoChanges, Lang.UserProfileTextError, MessageBoxButton.OK, MessageBoxImage.Stop);
                    txtUsername.Text = _originalUsername;
                    return;
                }

                string oldUsername = _currentUserData.Username;
                int oldChangeCount = _currentUserData.NameChangeCount;

                _currentUserData.Username = newUsername;
                _currentUserData.FacebookHandle = txtFacebookLink.Text.Trim();
                _currentUserData.XHandle = txtXLink.Text.Trim();
                _currentUserData.InstagramHandle = txtInstagramLink.Text.Trim();

                if (usernameChanged)
                {
                    _currentUserData.NameChangeCount++;
                }

                var client = SessionManager.UserClient;
                bool success = await client.SaveUserProfileAsync(_currentUserData);

                if (success)
                {
                    _originalUsername = newUsername;
                    SessionManager.CurrentUsername = newUsername; 
                    UpdateUsernameWarning(_currentUserData.NameChangeCount);
                    UpdateSocialMediaLinks();

                    MessageBox.Show(Lang.UserProfileTextSuccess, Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(Lang.UserProfileTextErrorSaving, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    _currentUserData.Username = oldUsername;
                    _currentUserData.NameChangeCount = oldChangeCount;
                    txtUsername.Text = _originalUsername;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                saveButton.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        private async void AvatarPage_AvatarSelected(object sender, string newAvatarId)
        {
            if (sender is Views.AvatarSelectionPage avatarPage)
                avatarPage.AvatarSelected -= AvatarPage_AvatarSelected;

            if (newAvatarId == _currentUserData.AvatarId) return;

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                var client = SessionManager.UserClient;

                bool success = await client.UpdateUserAvatarAsync(SessionManager.CurrentUsername, newAvatarId);

                if (success)
                {
                    _currentUserData.AvatarId = newAvatarId;

                    this.DataContext = null;
                    this.DataContext = _currentUserData;

                    MessageBox.Show(Lang.UserProfileTextAvatarSuccess, Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(Lang.UserProfileTextAvatarError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }


        private void UpdateUsernameWarning(int count)
        {
            int remaining = MAX_CHANGES - count;
            txtUsernameWarning.Text = string.Format(Lang.UserProfileTextUsernameChangesWarning, remaining, MAX_CHANGES);
            txtUsernameWarning.Visibility = Visibility.Visible;

            if (remaining <= 0)
            {
                txtUsernameWarning.Text = Lang.UserProfileTextChangesError;
                txtUsernameWarning.Foreground = new SolidColorBrush(Colors.Red);
                txtUsername.IsReadOnly = true;
            }
            else if (remaining == 1)
            {
                txtUsernameWarning.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                txtUsernameWarning.Foreground = new SolidColorBrush(Colors.White); 
            }
        }

        private void UpdateSocialMediaLinks()
        {
            linkFacebookContainer.Visibility = string.IsNullOrWhiteSpace(txtFacebookLink.Text.Trim()) ? Visibility.Collapsed : Visibility.Visible;
            linkXContainer.Visibility = string.IsNullOrWhiteSpace(txtXLink.Text.Trim()) ? Visibility.Collapsed : Visibility.Visible;
            linkInstagramContainer.Visibility = string.IsNullOrWhiteSpace(txtInstagramLink.Text.Trim()) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void ClickChangePassword(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de cambio de contraseña pendiente de implementación en el servidor.", "Pendiente", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClickChangeAvatar(object sender, RoutedEventArgs e)
        {
            if (_currentUserData == null) return;

            var avatarPage = new Views.AvatarSelectionPage(AvailableAvatars, _currentUserData.AvatarId);
            avatarPage.AvatarSelected += AvatarPage_AvatarSelected;
            NavigationService.Navigate(avatarPage);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string handle = "";
            string baseUrl = "";

            if (sender == linkFacebook)
            {
                handle = txtFacebookLink.Text.Trim();
                baseUrl = "https://www.facebook.com/";
            }
            else if (sender == linkX)
            {
                handle = txtXLink.Text.Trim();
                baseUrl = "https://x.com/";
            }
            else if (sender == linkInstagram)
            {
                handle = txtInstagramLink.Text.Trim();
                baseUrl = "https://www.instagram.com/";
            }

            if (!string.IsNullOrWhiteSpace(handle))
            {
                string finalUrl = baseUrl + handle;
                try
                {
                    if (Uri.IsWellFormedUriString(finalUrl, UriKind.Absolute))
                        Process.Start(new ProcessStartInfo(finalUrl) { UseShellExecute = true });
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}