using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TrucoPrueba1
{
    public static class MusicManager
    {
        private static MediaPlayer _player = new MediaPlayer();
        private static string _currentTrack = "";

        public static double Volume
        {
            get => _player.Volume;
            set
            {
                _player.Volume = Clamp(value, 0.0, 1.0);
            }
        }

        public static void Play(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
                return;

            if (_currentTrack == resourcePath)
                return;

            _currentTrack = resourcePath;

            try
            {
                _player.Open(new Uri(resourcePath, UriKind.Absolute));
                _player.Volume = 0.5;
                _player.MediaEnded -= LoopHandler;
                _player.MediaEnded += LoopHandler;
                _player.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al reproducir música: {ex.Message}");
            }
        }

        public static void Stop()
        {
            _player.Stop();
            _currentTrack = "";
        }

        public static void ChangeTrack(string resourcePath)
        {
            Stop();
            Play(resourcePath);
        }

        private static void LoopHandler(object sender, EventArgs e)
        {
            _player.Position = TimeSpan.Zero;
            _player.Play();
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
