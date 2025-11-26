using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.Properties.Langs;
using TrucoClient.Views;

namespace TrucoClient.Helpers.UI
{
    public static class InputRestriction
    {
        private const string MESSAGE_ERROR = "Error";
        public static void AttachRegexValidation(TextBox textBox, Regex allowedCharacters)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException(nameof(textBox));
            }

            if (allowedCharacters == null)
            {
                throw new ArgumentNullException(nameof(allowedCharacters));
            }

            textBox.PreviewTextInput += (sender, e) =>
            {
                string normalized;

                try
                {
                    normalized = e.Text.Normalize(NormalizationForm.FormC);
                }
                catch (ArgumentException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Handled = true;
                    return;
                }
                catch (Exception)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Handled = true;
                    return;
                }

                e.Handled = !allowedCharacters.IsMatch(normalized);
            };

            textBox.PreviewKeyDown += (sender, e) =>
            {
                if (e.Key == Key.Space)
                {
                    e.Handled = true;
                }
            };

            DataObject.AddPastingHandler(textBox, (sender, e) =>
            {
                try
                {
                    if (!e.DataObject.GetDataPresent(DataFormats.Text))
                    {
                        e.CancelCommand();
                        return;
                    }

                    string pasteText = e.DataObject.GetData(DataFormats.Text) as string;

                    if (pasteText == null)
                    {
                        e.CancelCommand();
                        return;
                    }

                    string normalized = pasteText.Normalize(NormalizationForm.FormC);

                    if (!allowedCharacters.IsMatch(normalized))
                    {
                        e.CancelCommand();
                    }
                }
                catch (ArgumentNullException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
                catch (ArgumentException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
                catch (RegexMatchTimeoutException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
                catch (ExternalException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
                catch (SecurityException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
                catch (Exception)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
            });
        }

        public static void AttachRegexValidation(PasswordBox passwordBox, Regex allowedCharacters)
        {
            if (passwordBox == null)
            {
                throw new ArgumentNullException(nameof(passwordBox));
            }

            if (allowedCharacters == null)
            {
                throw new ArgumentNullException(nameof(allowedCharacters));
            }

            passwordBox.PreviewTextInput += (sender, e) =>
            {
                string normalized;

                try
                {
                    normalized = e.Text.Normalize(NormalizationForm.FormC);
                }
                catch (ArgumentException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Handled = true;
                    return;
                }
                catch (Exception)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Handled = true;
                    return;
                }

                e.Handled = !allowedCharacters.IsMatch(normalized);
            };

            passwordBox.PreviewKeyDown += (sender, e) =>
            {
                if (e.Key == Key.Space)
                {
                    e.Handled = true;
                }
            };

            DataObject.AddPastingHandler(passwordBox, (sender, e) =>
            {
                try
                {
                    if (!e.DataObject.GetDataPresent(DataFormats.Text))
                    {
                        e.CancelCommand();
                        return;
                    }

                    string pasteText = e.DataObject.GetData(DataFormats.Text) as string;

                    if (pasteText == null)
                    {
                        e.CancelCommand();
                        return;
                    }

                    string normalized = pasteText.Normalize(NormalizationForm.FormC);

                    if (!allowedCharacters.IsMatch(normalized))
                    {
                        e.CancelCommand();
                    }
                }
                catch (ArgumentNullException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
                catch (ArgumentException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
                catch (RegexMatchTimeoutException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
                catch (ExternalException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
                catch (SecurityException)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
                catch (Exception)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
            });
        }
    }
}
