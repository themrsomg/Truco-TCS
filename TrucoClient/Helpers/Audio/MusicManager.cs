using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using TrucoClient.Properties.Langs;
using TrucoClient.Views;

namespace TrucoClient.Helpers.Audio
{
    public static class MusicManager
    {
        private const double VOLUME_EPSILON = 0.000001;
        private const string MESSAGE_ERROR = "Error";
        private const string MENU_MUSIC_FILE_NAME = "music_in_menus.mp3";

        private static MediaPlayer player = new MediaPlayer();
        private static string currentTrack = string.Empty;
        private static double lastVolume = 0.3;
        public static bool IsMuted => Math.Abs(player.Volume - 0.0) < VOLUME_EPSILON;

        public static double Volume
        {
            get => player.Volume;
            set
            {
                player.Volume = Clamp(value, 0.0, 1.0);
                if (value > 0)
                {
                    lastVolume = player.Volume;
                }
            }
        }

        public static void ToggleMute()
        {
            if (IsMuted)
                player.Volume = lastVolume;
            else
            {
                lastVolume = player.Volume;
                player.Volume = 0.0;
            }
        }

        public static void Play(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return;
            }
            try
            {
                string fullPath = Path.IsPathRooted(resourcePath)
                    ? resourcePath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resourcePath);

                if (!File.Exists(fullPath))
                {
                    CustomMessageBox.Show(Lang.ExceptionTextFileNotFound, 
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                }

                if (currentTrack == fullPath)
                {
                    return;
                }

                currentTrack = fullPath;
                player.Open(new Uri(fullPath, UriKind.Absolute));
                player.Volume = 0.5;
                player.MediaEnded -= LoopHandler;
                player.MediaEnded += LoopHandler;
                player.MediaFailed -= OnMediaFailed;
                player.MediaFailed += OnMediaFailed;
                player.Play();

            }
            catch (ArgumentException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorPlayingMusic, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void OnMediaFailed(object sender, ExceptionEventArgs e)
        {
            CustomMessageBox.Show(Lang.ExceptionTextErrorPlayingMusic, 
                MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool IsMenuMusicPlaying()
        {
            return player.Source != null &&
                   player.Source.AbsolutePath.EndsWith(MENU_MUSIC_FILE_NAME, StringComparison.OrdinalIgnoreCase);
        }

        public static void Stop()
        {
            try
            {
                player.Stop();
                player.Close();
                currentTrack = string.Empty;
            }
            catch (ArgumentException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorPlayingMusic, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void ChangeTrack(string resourcePath)
        {
            Stop();
            Play(resourcePath);
        }

        private static void LoopHandler(object sender, EventArgs e)
        {
            player.Position = TimeSpan.Zero;
            player.Play();
        }

        private static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }

    public static class MusicInitializer
    {
        private const string MENU_MUSIC_FILE_NAME = "music_in_menus.mp3";
        private const string START_MUSIC_FILE_NAME = "music_in_start.mp3";
        private const double DEFAULT_VOLUME = 0.3;
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
                splashPlayer.Volume = 0.6;
                splashPlayer.Play();
            }
            catch (UriFormatException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorPlayingMusic, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return splashPlayer;
        }
    }
}