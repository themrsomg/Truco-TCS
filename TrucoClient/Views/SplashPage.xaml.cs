using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TrucoClient.Views
{
    public partial class SplashPage : Page
    {
        private MediaPlayer splashPlayer;

        public SplashPage()
        {
            InitializeComponent();
            this.Loaded += OnSplashPageLoaded;
        }

        private void OnSplashPageLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnSplashPageLoaded;
            splashPlayer = MusicInitializer.InitializeSplashMusic();
            StartLogoAnimation();
            _ = NavigateAfterDelayAsync(TimeSpan.FromSeconds(5.4));
        }

        private void StartLogoAnimation()
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1.2));
            imgGameLogo.BeginAnimation(OpacityProperty, fadeIn);

            var scaleUp = new DoubleAnimation(0.5, 1.0, TimeSpan.FromSeconds(1.2))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            if (imgGameLogo.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);
            }

            var textFadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1.0))
            {
                BeginTime = TimeSpan.FromSeconds(1.5)
            };

            if (this.Content is Grid rootGrid && rootGrid.Children.Count > 1 && rootGrid.Children[1] is TextBlock textBlock)
            {
                textBlock.BeginAnimation(OpacityProperty, textFadeIn);
            }
        }

        private async Task NavigateAfterDelayAsync(TimeSpan delay)
        {
            await Task.Delay(delay);
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            fadeOut.Completed += (s, e) =>
            {
                try
                {
                    splashPlayer?.Stop();
                    splashPlayer?.Close();
                    splashPlayer = null;
                    MusicInitializer.InitializeMenuMusic();
                    this.NavigationService?.Navigate(new StartPage());
                }
                catch (AnimationException ex)
                {
                    MessageBox.Show($"Error de Animación al finalizar SplashPage {ex.Message}", "Error de Animación", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al finalizar SplashPage: {ex.Message}");
                }
            };

            if (this.Content is UIElement rootElement)
            {
                rootElement.BeginAnimation(OpacityProperty, fadeOut);
            }
            else
            {
                splashPlayer?.Stop();
                splashPlayer?.Close();
                splashPlayer = null;
                MusicInitializer.InitializeMenuMusic();
                this.NavigationService?.Navigate(new StartPage());
            }
        }
    }
}