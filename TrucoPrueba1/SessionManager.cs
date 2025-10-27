using System;
using System.Threading.Tasks;
using System.Windows;
using TrucoClient.TrucoServer;

namespace TrucoClient
{
    public static class SessionManager
    {
        public static string CurrentUsername { get; set; } = "UsuarioActual";
        public static UserProfileData CurrentUserData { get; set; }
        public static async Task<string> ResolveUsernameAsync(string usernameOrEmail)
        {
            if (!usernameOrEmail.Contains("@"))
            {
                return usernameOrEmail;
            }

            try
            {
                var profile = await ClientManager.UserClient.GetUserProfileByEmailAsync(usernameOrEmail);

                if (profile != null && !string.IsNullOrWhiteSpace(profile.Username))
                {
                    return profile.Username;
                }
            }
            catch (System.ServiceModel.EndpointNotFoundException ex)
            {
                MessageBox.Show($"No se pudo conectar al servidor: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return usernameOrEmail;
        }
        public static void ClearSession()
        {
            CurrentUserData = null;
            CurrentUsername = "UsuarioActual";
            ClientManager.CloseAllClients();
        }
    }
}
