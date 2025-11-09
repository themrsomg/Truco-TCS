using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Audio;

namespace TrucoClient.Views
{
    public partial class AvatarSelectionPage : Page
    {
        private const int AVATAR_SIZE = 80;
        private const int MARGIN = 6;

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

            foreach (string avatarId in avatars)
            {
                Button avatarButton = CreateAvatarButton(avatarId);
                AvatarsPanel.Children.Add(avatarButton);
            }
        }

        private Button CreateAvatarButton(string avatarId)
        {
            var button = new Button
            {
                Width = AVATAR_SIZE,
                Height = AVATAR_SIZE,
                Margin = new Thickness(MARGIN),
                Tag = avatarId,
                Cursor = Cursors.Hand,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };

            var image = new Image
            {
                Width = AVATAR_SIZE,
                Height = AVATAR_SIZE,
                Stretch = System.Windows.Media.Stretch.UniformToFill
            };

            AvatarHelper.LoadAvatarImage(image, avatarId);
            button.Content = image;

            if (currentSelectedId == avatarId)
            {
                button.Background = System.Windows.Media.Brushes.LightGray;
            }

            button.Click += ClickAvatar;
            return button;
        }

        private void ClickAvatar(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string id)
            {
                currentSelectedId = id;

                foreach (UIElement child in AvatarsPanel.Children)
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
