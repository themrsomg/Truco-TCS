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

        public static void AttachTextBoxValidation(TextBox textBox, string pattern)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("TextBox");
            }

            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Pattern cannot be null or empty");
            }

            Regex allowed;
            try
            {
                allowed = new Regex(pattern, RegexOptions.Compiled);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("Invalid regex pattern: " + ex.Message, MESSAGE_ERROR, ex);
            }

            textBox.PreviewTextInput += (sender, e) =>
            {
                try
                {
                    string normalized = (e.Text ?? string.Empty).Normalize(NormalizationForm.FormC);
                    e.Handled = !allowed.IsMatch(normalized);
                }
                catch (ArgumentException)
                {
                    e.Handled = true;
                }
                catch (RegexMatchTimeoutException)
                {
                    e.Handled = true;
                }
                catch (Exception)
                {
                    MessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Handled = true;
                }
            };

            textBox.PreviewKeyDown += (sender, e) =>
            {
                if (e.Key == Key.Space)
                {
                    if (!allowed.IsMatch(" "))
                    {
                        e.Handled = true;
                    }
                }
            };

            DataObject.AddPastingHandler(textBox, (sender, e) =>
            {
                try
                {
                    if (e == null || e.DataObject == null)
                    {
                        e?.CancelCommand();
                        return;
                    }

                    if (!e.DataObject.GetDataPresent(DataFormats.Text))
                    {
                        e.CancelCommand();
                        return;
                    }

                    object data = e.DataObject.GetData(DataFormats.Text);
                    if (data == null)
                    {
                        e.CancelCommand();
                        return;
                    }

                    string pasteText = data.ToString();
                    if (string.IsNullOrWhiteSpace(pasteText))
                    {
                        e.CancelCommand();
                        return;
                    }

                    string normalized = pasteText.Normalize(NormalizationForm.FormC);

                    bool allowedMatch;
                    try
                    {
                        allowedMatch = allowed.IsMatch(normalized);
                    }
                    catch (ArgumentException)
                    {
                        allowedMatch = false;
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        allowedMatch = false;
                    }

                    if (!allowedMatch)
                    {
                        e.CancelCommand();
                    }
                }
                catch (ArgumentNullException)
                {
                    e.CancelCommand();
                }
                catch (ArgumentException)
                {
                    e.CancelCommand();
                }
                catch (RegexMatchTimeoutException)
                {
                    e.CancelCommand();
                }
                catch (ExternalException)
                {
                    MessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                    e.CancelCommand();
                }
                catch (SecurityException)
                {
                    MessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                    e.CancelCommand();
                }
                catch (Exception)
                {
                    MessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.CancelCommand();
                }
            });
        }

        public static string RestrictToAllowedCharacters(string input, Regex allowed)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();

            foreach (char c in input)
            {
                string s = c.ToString();
                if (allowed.IsMatch(s))
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

    }
}
