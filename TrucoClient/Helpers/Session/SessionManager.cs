using System;
using System.Threading.Tasks;
using System.Windows;
using TrucoClient.TrucoServer;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.Services;

namespace TrucoClient.Helpers.Session
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
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorResolvingUser, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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