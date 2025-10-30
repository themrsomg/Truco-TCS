using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TrucoClient.Views
{
    public partial class AvatarSelectionPage : Page
    {
        private const int WIDTH_SIZE = 80;
        private const int HEIGHT_SIZE = 80;
        private const int MARGIN_TICKNESS_SIZE = 6;
        private const int PADDING_TICKNESS_SIZE = 0;
        private const int BORDER_TICKNESS_SIZE = 0;
        private const String URL_AVATAR_DEFAULT = "pack://application:,,,/TrucoClient;component/Resources/Avatars/avatar_default.png";

        public event EventHandler<string> AvatarSelected;

        private readonly List<string> avatars;
        private string currentSelectedId;
        public AvatarSelectionPage(List<string> availableAvatars, string currentId)
        {
            InitializeComponent();
            avatars = availableAvatars ?? new List<string>();
            currentSelectedId = currentId;
            PopulateAvatars();
        }

        private void PopulateAvatars()
        {
            AvatarsPanel.Children.Clear();

            foreach (var avatarId in avatars)
            {
                var btnAvatar = new Button
                {
                    Width = WIDTH_SIZE,
                    Height = HEIGHT_SIZE,
                    Margin = new Thickness(MARGIN_TICKNESS_SIZE),
                    Padding = new Thickness(PADDING_TICKNESS_SIZE),
                    Tag = avatarId,
                    Cursor = Cursors.Hand,
                    Background = System.Windows.Media.Brushes.Transparent,
                    BorderThickness = new Thickness(BORDER_TICKNESS_SIZE)
                };

                string packUri = $"pack://application:,,,/TrucoClient;component/Resources/Avatars/{avatarId}.png";

                var image = new System.Windows.Controls.Image
                {
                    Width = WIDTH_SIZE,
                    Height = HEIGHT_SIZE,
                    Stretch = System.Windows.Media.Stretch.UniformToFill
                };

                try
                {
                    image.Source = new BitmapImage(new Uri(packUri, UriKind.Absolute));
                }
                catch
                {
                    try
                    {
                        image.Source = new BitmapImage(new Uri(URL_AVATAR_DEFAULT, UriKind.Absolute));
                    }
                    catch (System.UriFormatException ex)
                    {
                        MessageBox.Show($"El formato es inválido para {avatarId}. Detalles: {ex.Message}");
                        LoadDefaultAvatar(image, avatarId);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Fallo al cargar el avatar {avatarId}. Detalles: {ex.Message}");
                        LoadDefaultAvatar(image, avatarId);
                    }
                }

                btnAvatar.Content = image;

                if (!string.IsNullOrEmpty(currentSelectedId) && currentSelectedId == avatarId)
                {
                    btnAvatar.Background = System.Windows.Media.Brushes.LightGray;
                }

                btnAvatar.Click += ClickAvatar;
                AvatarsPanel.Children.Add(btnAvatar);
            }
        }
        private void LoadDefaultAvatar(System.Windows.Controls.Image imageControl, string avatarId)
        {
            try
            {
                imageControl.Source = new BitmapImage(new Uri(URL_AVATAR_DEFAULT, UriKind.Absolute));
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Fallo al cargar la imagen por defecto '{URL_AVATAR_DEFAULT}'. Detalles: {ex.Message}");
            }
        }

        private void ClickAvatar(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string id)
            {
                currentSelectedId = id;

                foreach (var child in AvatarsPanel.Children)
                {
                    if (child is Button btn)
                    {
                        btn.Background = System.Windows.Media.Brushes.Transparent;
                    }
                }
                button.Background = System.Windows.Media.Brushes.LightGray;
            }
        }
        private void ClickBack(object sender, RoutedEventArgs e)
        {
            AvatarSelected?.Invoke(this, currentSelectedId);
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }
    }
}
