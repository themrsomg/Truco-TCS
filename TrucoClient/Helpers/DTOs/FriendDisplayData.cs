using System;
using System.Windows;
using System.Windows.Media.Imaging;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Paths;
using TrucoClient.Properties.Langs;
using TrucoClient.Views;

namespace TrucoClient.Helpers.DTOs
{
    public class FriendDisplayData
    {
        private const string MESSAGE_ERROR = "Error";
        private const string AVATAR_DEFAULT_ID = "avatar_aaa_default";
        public string Username { get; set; }
        public string AvatarId { get; set; }

        public string AvatarImagePath
        {
            get
            {
                string id = string.IsNullOrWhiteSpace(AvatarId) ? AVATAR_DEFAULT_ID : AvatarId;
                string correctedPath = $"/Resources/Avatars/{id}.png";

                try
                {
                    _ = new BitmapImage(new Uri(correctedPath, UriKind.Relative));
                    return correctedPath;
                }
                catch (UriFormatException ex)
                {
                    ClientException.HandleError(ex, nameof(AvatarImagePath));
                    CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    return ResourcePaths.DEFAULT_AVATAR_PATH;
                }
                catch (Exception ex)
                {
                    ClientException.HandleError(ex, nameof(AvatarImagePath));
                    CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    return ResourcePaths.DEFAULT_AVATAR_PATH;
                }
            }
        }
    }
}
