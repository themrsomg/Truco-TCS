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
        // Evento que devuelve el avatar seleccionado al volver
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
                var btn = new Button
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

                string packUri = $"pack://application:,,,/TrucoPrueba1;component/Resources/Avatars/{avatarId}.webp";

                var img = new System.Windows.Controls.Image
                {
                    Width = 80,
                    Height = 80,
                    Stretch = System.Windows.Media.Stretch.UniformToFill
                };

                try
                {
                    img.Source = new BitmapImage(new Uri(packUri, UriKind.Absolute));
                }
                catch
                {
                    try
                    {
                        img.Source = new BitmapImage(new Uri("pack://application:,,,/TrucoPrueba1;component/Resources/Avatars/avatar_default.webp", UriKind.Absolute));
                    }
                    catch 
                    {

                    }
                }

                btn.Content = img;

                if (!string.IsNullOrEmpty(currentSelectedId) && currentSelectedId == avatarId)
                {
                    btn.Background = System.Windows.Media.Brushes.LightGray;
                }

                btn.Click += AvatarButton_Click;
                AvatarsPanel.Children.Add(btn);
            }
        }

        private void AvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is string id)
            {
                currentSelectedId = id;

                foreach (var child in AvatarsPanel.Children)
                {
                    if (child is Button btn)
                    {
                        btn.Background = System.Windows.Media.Brushes.Transparent;
                    }
                }
                b.Background = System.Windows.Media.Brushes.LightGray;
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
