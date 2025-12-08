using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Markup;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.Audio;

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

            if (this.Content is UIElement rootElement)
            {
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));

                fadeOut.Completed += (s, e) => FinalizeSplash();

                try
                {
                    rootElement.BeginAnimation(OpacityProperty, fadeOut);
                }
                catch (AnimationException)
                {
                    FinalizeSplash();
                }
            }
            else
            {
                FinalizeSplash();
            }
        }

        private void FinalizeSplash()
        {
            try
            {
                if (splashPlayer != null)
                {
                    splashPlayer.Stop();
                    splashPlayer.Close();
                    splashPlayer = null;
                }

                MusicInitializer.InitializeMenuMusic();

                if (this.NavigationService != null)
                {
                    this.NavigationService.Navigate(new StartPage());
                }
            }
            catch (XamlParseException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextSplashAnimationError,
                    Lang.DialogTextError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextSplashAnimationError,
                    Lang.DialogTextError, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (AnimationException)
            {
                CustomMessageBox.Show(Lang.ExceptionTextSplashAnimationError,
                    Lang.GlobalTextAnimationError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextSplashAnimationError,
                    Lang.DialogTextError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}