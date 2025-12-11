using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Properties.Langs;
using TrucoClient.Views;

namespace TrucoClient.Helpers.Audio
{
    public static class MusicInitializer
    {
        private const string MENU_MUSIC_FILE_NAME = "music_in_menus.mp3";
        private const string START_MUSIC_FILE_NAME = "music_in_start.mp3";
        private const double DEFAULT_VOLUME = 0.3;
        private const double SPLASH_PAGE_VOLUME = 0.6;
        private const string MESSAGE_ERROR = "Error";
        private const string RESOURCES_NAME = "Resources";
        private const string SONGS_NAME = "Songs";

        public static void InitializeMenuMusic()
        {
            if (MusicManager.IsMenuMusicPlaying())
            {
                return;
            }

            string trackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RESOURCES_NAME, SONGS_NAME, MENU_MUSIC_FILE_NAME);

            MusicManager.Play(trackPath);
            MusicManager.Volume = DEFAULT_VOLUME;

            if (Properties.Settings.Default.IsMusicMuted)
            {
                MusicManager.ToggleMute();
            }
        }

        public static MediaPlayer InitializeSplashMusic()
        {
            string splashPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RESOURCES_NAME, SONGS_NAME, START_MUSIC_FILE_NAME);

            var splashPlayer = new MediaPlayer();

            try
            {
                splashPlayer.Open(new Uri(splashPath, UriKind.Absolute));
                splashPlayer.Volume = SPLASH_PAGE_VOLUME;
                splashPlayer.Play();
            }
            catch (UriFormatException ex)
            {
                ClientException.HandleError(ex, nameof(InitializeSplashMusic));
                CustomMessageBox.Show(Lang.ExceptionTextErrorPlayingMusic,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(InitializeSplashMusic));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return splashPlayer;
        }
    }
}
