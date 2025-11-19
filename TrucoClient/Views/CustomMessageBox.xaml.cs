using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class CustomMessageBox : Window
    {
        private const string CONFIRM_BUTTON_STYLE_KEY = "ConfirmButtonStyle";
        private const string CANCEL_BUTTON_STYLE_KEY = "CancelButtonStyle";
        private const string INFORMATION_IMAGE_FILE_NAME = "alert_information.png";
        private const string ERROR_IMAGE_FILE_NAME = "alert_error.png";
        private const string WARNING_IMAGE_FILE_NAME = "alert_warning.png";
        private const string QUESTION_IMAGE_FILE_NAME = "alert_question.png";
        private CustomMessageBox(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            InitializeComponent();

            txtMessage.Text = message;
            this.Title = caption;

            SetAlertImage(icon);

            CreateButtons(buttons);

            if (Application.Current != null && Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
            {
                this.Owner = Application.Current.MainWindow;
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void SetAlertImage(MessageBoxImage icon)
        {
            string imageName = INFORMATION_IMAGE_FILE_NAME;

            switch (icon)
            {
                case MessageBoxImage.Error:
                    imageName = ERROR_IMAGE_FILE_NAME;
                    break;

                case MessageBoxImage.Question:
                    imageName = QUESTION_IMAGE_FILE_NAME;
                    break;

                case MessageBoxImage.Warning:
                    imageName = WARNING_IMAGE_FILE_NAME;
                    break;

                case MessageBoxImage.Information:

                default:
                    imageName = INFORMATION_IMAGE_FILE_NAME;
                    break;
            }

            try
            {
                var uriSource = new Uri($"pack://application:,,,/Resources/Alerts/{imageName}", UriKind.Absolute);

                var brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(uriSource);
                brush.Stretch = Stretch.Uniform;
                brdAlertBackground.Background = brush;
            }
            catch 
            {
                /* 
                 * The exception is ignored to prevent the application from crashing.
                 * If a visual resource is missing, the window will be displayed without a background image.
                 * But it will still be functional.
                 */
            }
        }

        private void CreateButtons(MessageBoxButton buttons)
        {
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    AddButton(Lang.GlobalTextAccept, true, true, CONFIRM_BUTTON_STYLE_KEY);
                    break;

                case MessageBoxButton.OKCancel:
                    AddButton(Lang.GlobalTextAccept, true, true, CONFIRM_BUTTON_STYLE_KEY);
                    AddButton(Lang.GlobalTextCancel, false, false, CANCEL_BUTTON_STYLE_KEY);
                    break;

                case MessageBoxButton.YesNo:
                    AddButton(Lang.GlobalTextYes, true, true, CONFIRM_BUTTON_STYLE_KEY);
                    AddButton(Lang.GlobalTextNo, false, false, CANCEL_BUTTON_STYLE_KEY);
                    break;

                case MessageBoxButton.YesNoCancel:
                    AddButton(Lang.GlobalTextYes, true, true, CONFIRM_BUTTON_STYLE_KEY);
                    AddButton(Lang.GlobalTextNo, false, false, CANCEL_BUTTON_STYLE_KEY);
                    AddButton(Lang.GlobalTextCancel, false, null, CANCEL_BUTTON_STYLE_KEY);
                    break;
            }
        }

        private void AddButton(string label, bool isDefault, bool? result, string styleKey)
        {
            Style buttonStyle = (Style)this.FindResource(styleKey);

            var button = new Button
            {
                Content = label,
                IsDefault = isDefault,
                Style = buttonStyle,
                Width = 140,
                Height = 50
            };

            button.Click += (sender, e) =>
            {
                this.DialogResult = result;
            };

            ButtonsPanel.Children.Add(button);
        }

        public static bool? Show(string messageBoxText, string caption = "Message", 
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            var msgWindow = new CustomMessageBox(messageBoxText, caption, button, icon);

            return msgWindow.ShowDialog();
        }
    }
}
