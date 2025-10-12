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

namespace TrucoPrueba1
{
    // --- LÓGICA DE SESIÓN Y CLIENTE WCF ---
    public static class SessionManager
    {
        // NOTA: Se asume que TrucoCallbackHandler y InstanceContext están definidos.
        private static readonly TrucoCallbackHandler callbackHandler = new TrucoCallbackHandler();
        private static readonly InstanceContext userContext = new InstanceContext(callbackHandler);
        private static TrucoUserServiceClient _userClient;

        // Propiedad robusta que repara el cliente WCF si falla (Faulted) o si es null.
        // **Esta es la implementación recomendada para mejorar la estabilidad de la conexión.**
        public static TrucoUserServiceClient UserClient
        {
            get
            {
                // Recrea el cliente solo si es nulo o si la conexión está fallida
                if (_userClient == null || _userClient.State == CommunicationState.Faulted)
                {
                    // Debes asegurarte de que el nombre del endpoint coincida con tu App.config
                    _userClient = new TrucoUserServiceClient(userContext, "NetTcpBinding_ITrucoUserService");
                }
                return _userClient;
            }
        }

        // Se asume que esta propiedad se establece al iniciar sesión
        public static string CurrentUsername { get; set; } = "UsuarioActual";
    }

    // --- CÓDIGO DE LA PÁGINA ---
    public partial class UserProfilePage : Page
    {
        private UserProfileData _currentUserData;

        // La lista de avatares disponibles es necesaria para la navegación
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
            // Llama a la carga del perfil al iniciar la página
            LoadUserProfile();
        }

        // Se convierte a async void para no bloquear la UI
        private async void LoadUserProfile()
        {
            if (string.IsNullOrWhiteSpace(SessionManager.CurrentUsername) ||
                SessionManager.CurrentUsername == "UsuarioActual")
            {
                // Podrías redirigir a LoginPage si no hay sesión
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                var client = SessionManager.UserClient;

                // Cargar perfil
                // Se usa GetUserProfileAsync que se genera al actualizar la referencia del servicio
                _currentUserData = await client.GetUserProfileAsync(SessionManager.CurrentUsername);

                if (_currentUserData == null) return;

                // Establecer el DataContext para que funcione el Binding del Avatar (AvatarId)
                this.DataContext = _currentUserData;
                _originalUsername = _currentUserData.Username;

                // Asignar valores a TextBoxes (no usan Binding para edición)
                txtUsername.Text = _currentUserData.Username;
                txtEmail.Text = _currentUserData.Email; // Es de solo lectura en XAML
                txtFacebookLink.Text = _currentUserData.FacebookHandle;
                txtXLink.Text = _currentUserData.XHandle;
                txtInstagramLink.Text = _currentUserData.InstagramHandle;

                UpdateUsernameWarning(_currentUserData.NameChangeCount);
                UpdateSocialMediaLinks();
            }
            catch (Exception ex)
            {
                // Muestra un error claro si la conexión falla (Común en WCF)
                MessageBox.Show($"Error de conexión al cargar el perfil: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        // Lógica de guardado asincrónica
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
                    MessageBox.Show("No puedes cambiar tu nombre de usuario. Has alcanzado el límite de 2 cambios.", "Error de Perfil", MessageBoxButton.OK, MessageBoxImage.Stop);
                    txtUsername.Text = _originalUsername;
                    return;
                }

                // 1. Actualizar el objeto local para enviarlo
                string oldUsername = _currentUserData.Username;
                int oldChangeCount = _currentUserData.NameChangeCount;

                _currentUserData.Username = newUsername;
                _currentUserData.FacebookHandle = txtFacebookLink.Text.Trim();
                _currentUserData.XHandle = txtXLink.Text.Trim();
                _currentUserData.InstagramHandle = txtInstagramLink.Text.Trim();

                // Si el nombre cambió, incrementamos el contador antes de enviarlo al servidor para que el servidor lo valide.
                if (usernameChanged)
                {
                    _currentUserData.NameChangeCount++;
                }

                var client = SessionManager.UserClient;
                // Se usa SaveUserProfileAsync, generado al actualizar la referencia
                bool success = await client.SaveUserProfileAsync(_currentUserData);

                if (success)
                {
                    // 2. Actualizar estado local y sesión solo si el servidor fue exitoso
                    _originalUsername = newUsername;
                    SessionManager.CurrentUsername = newUsername; // Actualizar la sesión global
                    UpdateUsernameWarning(_currentUserData.NameChangeCount);
                    UpdateSocialMediaLinks();

                    MessageBox.Show("Perfil guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Revertir los cambios si el servidor falla (ej: el nombre ya estaba tomado)
                    MessageBox.Show("Hubo un error al guardar el perfil. El nombre de usuario puede estar en uso o ya no quedan cambios.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Revertir el estado local
                    _currentUserData.Username = oldUsername;
                    _currentUserData.NameChangeCount = oldChangeCount;
                    txtUsername.Text = _originalUsername;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión al guardar: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                saveButton.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        // Método que se llama al seleccionar un avatar en la página AvatarSelectionPage
        private async void AvatarPage_AvatarSelected(object sender, string newAvatarId)
        {
            if (sender is Views.AvatarSelectionPage avatarPage)
                avatarPage.AvatarSelected -= AvatarPage_AvatarSelected;

            if (newAvatarId == _currentUserData.AvatarId) return;

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                var client = SessionManager.UserClient;

                // 1. Intentar actualizar en el servidor
                bool success = await client.UpdateUserAvatarAsync(SessionManager.CurrentUsername, newAvatarId);

                if (success)
                {
                    // 2. CORRECCIÓN CLAVE: Actualizamos la propiedad local
                    _currentUserData.AvatarId = newAvatarId;

                    // 3. Forzar el refresco del DataContext (el truco anti-caché)
                    this.DataContext = null;
                    this.DataContext = _currentUserData;

                    MessageBox.Show("Tu avatar ha sido actualizado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el avatar en el servidor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión al actualizar el avatar: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        // --- Métodos Auxiliares y Handlers de Clic ---

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
            else
            {
                txtUsernameWarning.Foreground = new SolidColorBrush(Colors.White); // Cambiado a blanco para mejor contraste en fondo oscuro
            }
        }

        private void UpdateSocialMediaLinks()
        {
            // Lógica para mostrar/ocultar los iconos de enlace si hay o no contenido en los TextBoxes
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
            // NOTA: La implementación completa del cambio de contraseña requiere más lógica.
            MessageBox.Show("Funcionalidad de cambio de contraseña pendiente de implementación en el servidor.", "Pendiente", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClickChangeAvatar(object sender, RoutedEventArgs e)
        {
            // Navegar a la página de selección de avatar
            if (_currentUserData == null) return;

            // 🚨 CORRECCIÓN: Usar el namespace completo o el using que tienes (TrucoPrueba1.Views)
            var avatarPage = new Views.AvatarSelectionPage(AvailableAvatars, _currentUserData.AvatarId);
            avatarPage.AvatarSelected += AvatarPage_AvatarSelected;
            NavigationService.Navigate(avatarPage);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // Lógica para abrir enlaces de redes sociales en el navegador
            // ... (El código anterior para esta función es correcto, no necesita cambios)
            // Ya que el XAML solo tiene el RequestNavigate, asumo que la implementación está bien.
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
                    MessageBox.Show($"No se pudo abrir el enlace: {ex.Message}", "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}