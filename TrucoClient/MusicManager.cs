using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media;

namespace TrucoClient
{
    public static class MusicManager
    {
        private static MediaPlayer player = new MediaPlayer();
        private static string currentTrack = "";
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
        public static bool IsMuted
        {
            get => player.Volume == 0.0;
        }
        public static void ToggleMute()
        {
            if (IsMuted)
            {
                player.Volume = lastVolume;
            }
            else
            {
                lastVolume = player.Volume;
                player.Volume = 0.0;
            }
        }
        public static void Play(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            { 
                return;
            }

            if (currentTrack == resourcePath)
            {
                return;
            }

            currentTrack = resourcePath;

            try
            {
                player.Open(new Uri(resourcePath, UriKind.Absolute));
                player.Volume = 0.5;
                player.MediaEnded -= LoopHandler;
                player.MediaEnded += LoopHandler;
                player.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al reproducir música: {ex.Message}");
            }
        }

        public static bool IsMenuMusicPlaying()
        {
            return player.Source != null && player.Source.AbsolutePath.EndsWith("music_in_menus.mp3", StringComparison.OrdinalIgnoreCase);
        }

        public static void Stop()
        {
            player.Stop();
            currentTrack = "";
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
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

    public static class MusicInitializer
    {
        private const string MenuMusicFileName = "music_in_menus.mp3";
        private const double DefaultVolume = 0.3;

        public static void InitializeMenuMusic()
        {
            if (MusicManager.IsMenuMusicPlaying())
            {
                return;
            }

            string trackPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "Songs",
                MenuMusicFileName
            );

            MusicManager.Play(trackPath);
            MusicManager.Volume = DefaultVolume;

            if (Properties.Settings.Default.IsMusicMuted)
            {
                MusicManager.ToggleMute();
            }
        }
    }
}
