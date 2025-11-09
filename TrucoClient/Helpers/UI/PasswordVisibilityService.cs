using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TrucoClient.Helpers.UI
{
    public static class PasswordVisibilityService
    {
        public static void ToggleVisibility(
            PasswordBox hiddenBox, TextBox visibleBox, TextBlock eyeIcon)
        {
            if (hiddenBox.Visibility == Visibility.Visible)
            {
                visibleBox.Text = hiddenBox.Password;
                hiddenBox.Visibility = Visibility.Collapsed;
                visibleBox.Visibility = Visibility.Visible;
                visibleBox.Focus();

                eyeIcon.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                hiddenBox.Password = visibleBox.Text;
                hiddenBox.Visibility = Visibility.Visible;
                visibleBox.Visibility = Visibility.Collapsed;
                hiddenBox.Focus();

                eyeIcon.Foreground = new SolidColorBrush(Colors.Black);
            }
        }
    }
}
