using System;
using System.Threading.Tasks;
using System.Windows;
using TrucoClient.TrucoServer;

namespace TrucoClient
{
    public static class SessionManager
    {
        public static string CurrentUsername { get; set; }
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al resolver usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return usernameOrEmail;
        }

        public static void Clear()
        {
            CurrentUserData = null;
            CurrentUsername = null;
            ClientManager.ResetConnections();
        }
    }
}