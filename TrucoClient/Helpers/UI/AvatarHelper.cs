using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Paths;
using TrucoClient.Properties.Langs;
using TrucoClient.Views;

namespace TrucoClient.Helpers.UI
{
    public static class AvatarHelper
    {
        private const string DEFAULT_AVATAR_ID = "avatar_aaa_default";
        private const string MESSAGE_ERROR = "Error";   
        private static readonly string defaultAvatarPackUri = GetPackUri(DEFAULT_AVATAR_ID);

        private static List<string> availableAvatars;

        public static IReadOnlyList<string> AvailableAvatars
        {
            get
            {
                if (availableAvatars == null)
                {
                    LoadAvatarList();
                }
                return availableAvatars.AsReadOnly();
            }
        }

        private static void LoadAvatarList()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                using (var stream = assembly.GetManifestResourceStream(ResourcePaths.MANIFEST_RESOURCE_NAME))
                
                using (var reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    availableAvatars = JsonConvert.DeserializeObject<List<string>>(json);
                }
            }
            catch (FileNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(LoadDefaultAvatar));
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextAvatarIdFailedToLoadDefault, defaultAvatarPackUri),
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);

                availableAvatars = new List<string>
                {
                    DEFAULT_AVATAR_ID
                };
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(LoadDefaultAvatar));
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextAvatarIdFailedToLoadDefault, defaultAvatarPackUri),
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void LoadAvatarImage(Image imageControl, string avatarId)
        {
            if (imageControl == null)
            {
                return;
            }

            string idToLoad = string.IsNullOrWhiteSpace(avatarId) ? DEFAULT_AVATAR_ID : avatarId;

            string packUri = GetPackUri(idToLoad);

            try
            {
                imageControl.Source = new BitmapImage(new Uri(packUri, UriKind.Absolute));
            }
            catch (UriFormatException)
            {
                imageControl.Source = new BitmapImage(new Uri(defaultAvatarPackUri, UriKind.Absolute));
            }
            catch (IOException)
            {
                LoadDefaultAvatar(imageControl);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(LoadAvatarImage));
                CustomMessageBox.Show(Lang.ExceptionTextErrorLoadingAvatar, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);

                imageControl.Source = new BitmapImage(new Uri(defaultAvatarPackUri, UriKind.Absolute));
            }
        }

        public static void LoadDefaultAvatar(Image imageControl)
        {
            try
            {
                imageControl.Source = new BitmapImage(new Uri(defaultAvatarPackUri, UriKind.Absolute));
            }
            catch (FileNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(LoadDefaultAvatar));
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextAvatarIdFailedToLoadDefault, defaultAvatarPackUri), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (UriFormatException ex)
            {
                ClientException.HandleError(ex, nameof(LoadDefaultAvatar));
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextAvatarIdFailedToLoadDefault, defaultAvatarPackUri), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(LoadDefaultAvatar));
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextAvatarIdFailedToLoadDefault, defaultAvatarPackUri), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static string GetPackUri(string avatarId)
        {
            return $"pack://application:,,,/TrucoClient;component{ResourcePaths.RESOURCE_BASE_PATH}{avatarId}.png";
        }
    }
}
