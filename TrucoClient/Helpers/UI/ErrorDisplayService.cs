using System.Windows.Controls;
using System.Windows.Media;

namespace TrucoClient.Helpers.UI
{
    public static class ErrorDisplayService
    {
        public static void ShowError(Control control, TextBlock block, string message)
        {
            if (block != null)
            {
                block.Text = message;
                block.Foreground = new SolidColorBrush(Colors.Red);
            }

            control.BorderBrush = new SolidColorBrush(Colors.Red);
        }

        public static void ClearError(Control control, TextBlock block)
        {
            if (block != null)
            {
                block.Text = string.Empty;
                block.Foreground = new SolidColorBrush(Colors.Red);
            }

            control.ClearValue(Border.BorderBrushProperty);
        }

        public static void ShowWarning(Control control, TextBlock block, string message)
        {
            if (block != null)
            {
                block.Text = message;
                block.Foreground = new SolidColorBrush(Colors.Orange);
            }
        }
    }
}
