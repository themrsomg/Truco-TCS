using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Documents;
using Microsoft.VisualBasic;
using System.Threading.Tasks;
using TrucoPrueba1.TrucoServer;
using System.ServiceModel;

namespace TrucoPrueba1
{
    public static class SessionManager
    {
        private static readonly TrucoCallbackHandler callbackHandler = new TrucoCallbackHandler();
        private static readonly InstanceContext userContext = new InstanceContext(callbackHandler);

        public static TrucoUserServiceClient UserClient { get; set; } =
            new TrucoUserServiceClient(userContext, "NetTcpBinding_ITrucoUserService");

        public static string CurrentUsername { get; set; } = "UsuarioActual";
    }

    public partial class UserProfilePage : Page
    {
        private UserProfileData _currentUserData;
        private const int MAX_CHANGES = 2;
        private string _originalUsername;

        public UserProfilePage()
        {
            InitializeComponent();
            LoadUserProfile();
        }

        private async void LoadUserProfile()
        {
            if (string.IsNullOrWhiteSpace(SessionManager.CurrentUsername) || SessionManager.CurrentUsername == "UsuarioActual")
            {
                MessageBox.Show("Debes iniciar sesión para ver tu perfil.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                Mouse.OverrideCursor = null;
                return;
            }
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                _currentUserData = await SessionManager.UserClient.GetUserProfileAsync(SessionManager.CurrentUsername);

                if (_currentUserData == null)
                {
                    MessageBox.Show("Error al cargar el perfil. Intenta de nuevo.", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

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
                MessageBox.Show($"Error de conexión al cargar el perfil: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void UpdateUsernameWarning(int count)
        {
            int remaining = MAX_CHANGES - count;
            txtUsernameWarning.Text = $"Te quedan {remaining} cambios de nombre. (Máximo {MAX_CHANGES})";
            txtUsernameWarning.Visibility = Visibility.Visible;

            if (remaining <= 0)
            {
                txtUsernameWarning.Text = "¡Ya no puedes cambiar tu nombre de usuario!";
                txtUsernameWarning.Foreground = new SolidColorBrush(Colors.Red);
                txtUsername.IsReadOnly = true;
            }
            else if (remaining == 1)
            {
                txtUsernameWarning.Foreground = new SolidColorBrush(Colors.Orange);
            }
        }

        private void UpdateSocialMediaLinks()
        {
            string fbHandle = txtFacebookLink.Text.Trim();
            string xHandle = txtXLink.Text.Trim();
            string instaHandle = txtInstagramLink.Text.Trim();

            linkFacebookContainer.Visibility = string.IsNullOrWhiteSpace(fbHandle) ? Visibility.Collapsed : Visibility.Visible;
            linkXContainer.Visibility = string.IsNullOrWhiteSpace(xHandle) ? Visibility.Collapsed : Visibility.Visible;
            linkInstagramContainer.Visibility = string.IsNullOrWhiteSpace(instaHandle) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private async void ClickSave(object sender, RoutedEventArgs e)
        {
            if (_currentUserData == null) return;

            string newUsername = txtUsername.Text.Trim();

            if (string.IsNullOrWhiteSpace(newUsername))
            {
                MessageBox.Show("El nombre de usuario no puede estar vacío.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Button saveButton = sender as Button;
            saveButton.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                bool usernameChanged = newUsername != _originalUsername;

                if (usernameChanged && _currentUserData.NameChangeCount >= MAX_CHANGES)
                {
                    MessageBox.Show("No puedes cambiar tu nombre de usuario. Has alcanzado el límite de 2 cambios.", "Error de Perfil", MessageBoxButton.OK, MessageBoxImage.Stop);
                    txtUsername.Text = _originalUsername;
                    return;
                }

                _currentUserData.Username = newUsername;
                _currentUserData.FacebookHandle = txtFacebookLink.Text.Trim();
                _currentUserData.XHandle = txtXLink.Text.Trim();
                _currentUserData.InstagramHandle = txtInstagramLink.Text.Trim();

                if (usernameChanged)
                {
                    _currentUserData.NameChangeCount++;
                }

                bool success = await SessionManager.UserClient.SaveUserProfileAsync(_currentUserData);

                if (success)
                {
                    _originalUsername = newUsername;
                    SessionManager.CurrentUsername = newUsername;
                    UpdateUsernameWarning(_currentUserData.NameChangeCount);
                    UpdateSocialMediaLinks();

                    MessageBox.Show("Perfil guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Hubo un error al guardar el perfil. El nombre de usuario puede estar en uso.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    if (usernameChanged)
                    {
                        _currentUserData.NameChangeCount--;
                    }
                    txtUsername.Text = _originalUsername;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error de WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                saveButton.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
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
                baseUrl = "https://www.x.com/";
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
                    Process.Start(new ProcessStartInfo(finalUrl) { UseShellExecute = true });
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo abrir el enlace: {ex.Message}", "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void ClickChangePassword(object sender, RoutedEventArgs e)
        {
            if (_currentUserData == null) return;

            MessageBoxResult confirmStart = MessageBox.Show(
                $"¿Estás seguro de que deseas cambiar tu contraseña? Se enviará un código de verificación al correo registrado: {_currentUserData.Email}.",
                "Confirmar Cambio",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmStart == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    bool codeSent = await SessionManager.UserClient.SendPasswordResetCodeAsync(SessionManager.CurrentUsername);

                    if (codeSent)
                    {
                        MessageBox.Show($"Se ha enviado un código de seguridad a {_currentUserData.Email}.", "Código Enviado", MessageBoxButton.OK, MessageBoxImage.Information);
                        ShowPasswordResetDialogFlow();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo enviar el código de seguridad. Intenta más tarde.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error de conexión: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private async void ShowPasswordResetDialogFlow()
        {
            string code = Interaction.InputBox("Introduce el código de verificación que recibiste por correo:", "Verificación", "");
            if (string.IsNullOrWhiteSpace(code)) return;

            string newPassword = Interaction.InputBox("Introduce tu nueva contraseña:", "Nueva Contraseña", "");
            if (string.IsNullOrWhiteSpace(newPassword)) return;

            string confirmPassword = Interaction.InputBox("Confirma tu nueva contraseña:", "Confirmar", "");
            if (string.IsNullOrWhiteSpace(confirmPassword)) return;

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Las contraseñas no coinciden. Intenta de nuevo.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult finalConfirm = MessageBox.Show(
                "¡ATENCIÓN! ¿Estás completamente seguro de que quieres cambiar tu contraseña?",
                "Confirmación Final",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (finalConfirm == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    bool resetSuccess = await SessionManager.UserClient.ResetPasswordAsync(SessionManager.CurrentUsername, code, newPassword);

                    if (resetSuccess)
                    {
                        MessageBox.Show("Contraseña cambiada exitosamente. Se recomienda iniciar sesión de nuevo.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("El código de verificación no es válido o hubo un error al cambiar la contraseña.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error de conexión: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }
    }
}