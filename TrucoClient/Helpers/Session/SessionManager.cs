using System;
using System.Threading.Tasks;
using System.Windows;
using TrucoClient.Helpers.Services;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;
using TrucoClient.Views;

namespace TrucoClient.Helpers.Session
{
    public static class SessionManager
    {
        private const string MESSAGE_ERROR = "Error";
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
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextErrorResolvingUser, ex.Message), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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