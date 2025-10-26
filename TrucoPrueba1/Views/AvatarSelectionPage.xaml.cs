using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TrucoPrueba1.Views
{
    public partial class AvatarSelectionPage : Page
    {
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
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(6),
                    Padding = new Thickness(0),
                    Tag = avatarId,
                    Cursor = Cursors.Hand,
                    Background = System.Windows.Media.Brushes.Transparent,
                    BorderThickness = new Thickness(0)
                };

                string packUri = $"pack://application:,,,/TrucoPrueba1;component/Resources/Avatars/{avatarId}.png";

                var image = new System.Windows.Controls.Image
                {
                    Width = 80,
                    Height = 80,
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
                        image.Source = new BitmapImage(new Uri("pack://application:,,,/TrucoPrueba1;component/Resources/Avatars/avatar_default.png", UriKind.Absolute));
                    }
                    catch 
                    {
                        
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
