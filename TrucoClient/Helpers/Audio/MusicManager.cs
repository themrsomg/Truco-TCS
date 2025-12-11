using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Properties.Langs;
using TrucoClient.Views;

namespace TrucoClient.Helpers.Audio
{
    public static class MusicManager
    {
        private const double VOLUME_EPSILON = 0.000001;
        private const string MESSAGE_ERROR = "Error";
        private const string MENU_MUSIC_FILE_NAME = "music_in_menus.mp3";
        private const double MIN_VOLUME = 0.0;
        private const double MAX_VOLUME = 1.0;
        private const double PLAYER_VOLUME = 0.5;

        private static MediaPlayer player = new MediaPlayer();
        private static string currentTrack = string.Empty;
        private static double lastVolume = 0.3;
        public static bool IsMuted => Math.Abs(player.Volume - MIN_VOLUME) < VOLUME_EPSILON;

        public static double Volume
        {
            get => player.Volume;
            set
            {
                player.Volume = Clamp(value, MIN_VOLUME, MAX_VOLUME);
                if (value > MIN_VOLUME)
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
                player.Volume = MIN_VOLUME;
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
                player.Volume = PLAYER_VOLUME;
                player.MediaEnded -= LoopHandler;
                player.MediaEnded += LoopHandler;
                player.MediaFailed -= OnMediaFailed;
                player.MediaFailed += OnMediaFailed;
                player.Play();

            }
            catch (ArgumentException ex)
            {
                ClientException.HandleError(ex, nameof(Play));
                CustomMessageBox.Show(Lang.ExceptionTextErrorPlayingMusic, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(Play));
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
            catch (ArgumentException ex)
            {
                ClientException.HandleError(ex, nameof(Stop));
                CustomMessageBox.Show(Lang.ExceptionTextErrorPlayingMusic, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(Stop));
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
}