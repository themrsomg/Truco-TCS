using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private static readonly Regex numericRegex = new Regex("[^0-9]+");
        public string InputResult { get; private set; }

        private CustomMessageBox(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon, bool isInputMode)
        {
            InitializeComponent();
            ConfigureWindow(caption);
            SetMessageAndInput(message, isInputMode);
            SetAlertImage(icon);
            CreateButtons(buttons);
        }

        public static string ShowInput(string messageBoxText, string caption = "Input", MessageBoxImage icon = MessageBoxImage.Question)
        {
            var msgWindow = new CustomMessageBox(messageBoxText, caption, MessageBoxButton.OKCancel, icon, isInputMode: true);
            var result = msgWindow.ShowDialog();

            return (result == true) ? msgWindow.InputResult : null;
        }

        public static bool? Show(string messageBoxText, string caption = "Message",
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            var msgWindow = new CustomMessageBox(messageBoxText, caption, button, icon, isInputMode: false);
            return msgWindow.ShowDialog();
        }

        private void ConfigureWindow(string caption)
        {
            this.Title = caption;

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

        private void SetMessageAndInput(string message, bool isInputMode)
        {
            blckMessage.Text = message;

            if (isInputMode)
            {
                txtInput.Visibility = Visibility.Visible;
                txtInput.Focus();
            }
            else
            {
                txtInput.Visibility = Visibility.Collapsed;
            }
        }

        private void SetAlertImage(MessageBoxImage icon)
        {
            string imageName = GetImageNameFromIcon(icon);
            LoadAndSetBackgroundImage(imageName);        
        }

        private string GetImageNameFromIcon(MessageBoxImage icon)
        {
            switch (icon)
            {
                case MessageBoxImage.Error: return ERROR_IMAGE_FILE_NAME;
                case MessageBoxImage.Question: return QUESTION_IMAGE_FILE_NAME;
                case MessageBoxImage.Warning: return WARNING_IMAGE_FILE_NAME;
                default: return INFORMATION_IMAGE_FILE_NAME;
            }
        }

        private void LoadAndSetBackgroundImage(string imageName)
        {
            try
            {
                var uriSource = new Uri($"pack://application:,,,/Resources/Alerts/{imageName}", UriKind.Absolute);
                var brush = new ImageBrush
                {
                    ImageSource = new BitmapImage(uriSource),
                    Stretch = Stretch.Uniform
                };
                brdAlertBackground.Background = brush;
            }
            catch (Exception)
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
                OnButtonClick(result);
            };

            ButtonsPanel.Children.Add(button);
        }

        private void OnButtonClick(bool? result)
        {
            if (txtInput.Visibility == Visibility.Visible && result == true)
            {
                if (!ValidateInput())
                {
                    return;
                }

                InputResult = txtInput.Text;
            }

            this.DialogResult = result;
        }

        private bool ValidateInput()
        {
            string text = txtInput.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                System.Media.SystemSounds.Beep.Play();
                txtInput.Focus();

                return false;
            }

            if (text.Length != 6)
            {
                CustomMessageBox.Show("The code must be 6 digits.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void InputPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }

        private void InputPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (IsTextNumeric(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private static bool IsTextNumeric(string text)
        {
            return numericRegex.IsMatch(text);
        }
    }
}
