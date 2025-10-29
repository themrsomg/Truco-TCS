using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.Windows;
using System.Windows.Media;

namespace TrucoClient
{
    public static class MusicManager
    {
        private static MediaPlayer player = new MediaPlayer();
        private static string currentTrack = string.Empty;
        private static double lastVolume = 0.3;

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

        public static bool IsMuted => player.Volume == 0.0;

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
                    MessageBox.Show($"Archivo no encontrado: {fullPath}");
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al reproducir música: {ex.Message}");
            }
        }

        private static void OnMediaFailed(object sender, ExceptionEventArgs e)
        {
            MessageBox.Show($"Fallo al reproducir música: {e.ErrorException?.Message}");
        }

        public static bool IsMenuMusicPlaying()
        {
            return player.Source != null &&
                   player.Source.AbsolutePath.EndsWith("music_in_menus.mp3", StringComparison.OrdinalIgnoreCase);
        }

        public static void Stop()
        {
            try
            {
                player.Stop();
                player.Close();
                currentTrack = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al detener música: {ex.Message}");
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
        private const double DEFAULT_VOLUME = 0.3;

        public static void InitializeMenuMusic()
        {
            if (MusicManager.IsMenuMusicPlaying())
            {
                return;
            }

            string trackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Songs", MENU_MUSIC_FILE_NAME);

            MusicManager.Play(trackPath);
            MusicManager.Volume = DEFAULT_VOLUME;

            if (Properties.Settings.Default.IsMusicMuted)
            {
                MusicManager.ToggleMute();
            }
        }

        public static MediaPlayer InitializeSplashMusic()
        {
            string splashPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Songs", "music_in_start.mp3");
            var splashPlayer = new MediaPlayer();

            try
            {
                splashPlayer.Open(new Uri(splashPath, UriKind.Absolute));
                splashPlayer.Volume = 0.6;
                splashPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al reproducir música del splash: {ex.Message}");
            }

            return splashPlayer;
        }
    }
}