using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class SplashPage : Page
    {
        private const double SPLASH_NAVIGATION_DELAY_SECONDS = 5.4;
        private const double LOGO_FADE_IN_FROM = 0.0;
        private const double LOGO_FADE_IN_TO = 1.0;
        private const double LOGO_FADE_IN_DURATION = 1.2;

        private const double LOGO_SCALE_FROM = 0.5;
        private const double LOGO_SCALE_TO = 1.0;
        private const double LOGO_SCALE_DURATION = 1.2;

        private const double TEXT_FADE_IN_FROM = 0.0;
        private const double TEXT_FADE_IN_TO = 1.0;
        private const double TEXT_FADE_IN_DURATION = 1.0;
        private const double TEXT_FADE_IN_BEGIN = 1.5;

        private const double FADE_OUT_FROM = 1.0;
        private const double FADE_OUT_TO = 0.0;
        private const double FADE_OUT_DURATION = 0.5;

        private const int ROOT_GRID_CHILDREN_INDEX = 1;

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
            _ = NavigateAfterDelayAsync(TimeSpan.FromSeconds(SPLASH_NAVIGATION_DELAY_SECONDS));
        }

        private void StartLogoAnimation()
        {
            var fadeIn = new DoubleAnimation(LOGO_FADE_IN_FROM, LOGO_FADE_IN_TO,
                TimeSpan.FromSeconds(LOGO_FADE_IN_DURATION));

            imgGameLogo.BeginAnimation(OpacityProperty, fadeIn);

            var scaleUp = new DoubleAnimation(LOGO_SCALE_FROM, LOGO_SCALE_TO,
                TimeSpan.FromSeconds(LOGO_SCALE_DURATION))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

            if (imgGameLogo.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);
            }

            var textFadeIn = new DoubleAnimation(TEXT_FADE_IN_FROM, TEXT_FADE_IN_TO,
                TimeSpan.FromSeconds(TEXT_FADE_IN_DURATION))
                {
                    BeginTime = TimeSpan.FromSeconds(TEXT_FADE_IN_BEGIN)
                };

            if (this.Content is Grid rootGrid && rootGrid.Children.Count > 1 &&
                rootGrid.Children[ROOT_GRID_CHILDREN_INDEX] is TextBlock textBlock)
            {
                textBlock.BeginAnimation(OpacityProperty, textFadeIn);
            }
        }

        private async Task NavigateAfterDelayAsync(TimeSpan delay)
        {
            await Task.Delay(delay);

            if (this.Content is UIElement rootElement)
            {
                var fadeOut = new DoubleAnimation(FADE_OUT_FROM, FADE_OUT_TO,
                    TimeSpan.FromSeconds(FADE_OUT_DURATION));

                fadeOut.Completed += (s, e) => FinalizeSplash();

                try
                {
                    rootElement.BeginAnimation(OpacityProperty, fadeOut);
                }
                catch (AnimationException)
                {
                    FinalizeSplash();
                    /**
                     * Intentionally left empty because an animation failure 
                     * should not block the navigation flow. If the animation 
                     * engine throws an AnimationException due to an invalid 
                     * visual state or a rendering issue, the application must 
                     * still proceed to FinalizeSplash() to avoid freezing the 
                     * UI or interrupting the startup sequence.
                     */
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
            catch (XamlParseException ex)
            {
                ClientException.HandleError(ex, nameof(FinalizeSplash));
                CustomMessageBox.Show(Lang.ExceptionTextSplashAnimationError,
                    Lang.DialogTextError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException ex)
            {
                ClientException.HandleError(ex, nameof(FinalizeSplash));
                CustomMessageBox.Show(Lang.ExceptionTextSplashAnimationError,
                    Lang.DialogTextError, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (AnimationException ex)
            {
                ClientException.HandleError(ex, nameof(FinalizeSplash));
                CustomMessageBox.Show(Lang.ExceptionTextSplashAnimationError,
                    Lang.GlobalTextAnimationError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(FinalizeSplash));
                CustomMessageBox.Show(Lang.ExceptionTextSplashAnimationError,
                    Lang.DialogTextError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
