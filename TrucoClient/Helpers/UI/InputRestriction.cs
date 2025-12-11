using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Properties.Langs;
using TrucoClient.Views;

namespace TrucoClient.Helpers.UI
{
    public static class InputRestriction
    {
        private const string MESSAGE_ERROR = "Error";
        private const double TIME_SPAM = 250;
        private static readonly TimeSpan regexTimeout = TimeSpan.FromMilliseconds(500);

        private static readonly Regex chatCharRegex = new Regex(@"^[A-Za-z0-9\-\._:/\?#\[\]@!$&'()*+,;=% ]$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(TIME_SPAM));

        public static void AttachRegexValidation(TextBox textBox, Regex allowedCharacters)
        {
            ValidateArguments(textBox, allowedCharacters);

            textBox.PreviewTextInput += (s, e) => HandlePreviewTextInput(e, allowedCharacters);
            textBox.PreviewKeyDown += HandlePreviewKeyDown;
            DataObject.AddPastingHandler(textBox, (s, e) => HandlePasting(e, allowedCharacters));
        }

        public static void AttachChatValidation(TextBox textBox, int maxCharacters)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException(nameof(textBox));
            }

            try
            {
                textBox.MaxLength = maxCharacters;

                AttachRegexValidation(textBox, chatCharRegex);
                textBox.PreviewKeyDown -= HandlePreviewKeyDown;
            }
            catch (ArgumentException ex)
            {
                ClientException.HandleError(ex, nameof(AttachChatValidation));
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(AttachChatValidation));
            }
        }

        public static void AttachRegexValidation(PasswordBox passwordBox, Regex allowedCharacters)
        {
            ValidateArguments(passwordBox, allowedCharacters);

            passwordBox.PreviewTextInput += (s, e) => HandlePreviewTextInput(e, allowedCharacters);
            passwordBox.PreviewKeyDown += HandlePreviewKeyDown;
            DataObject.AddPastingHandler(passwordBox, (s, e) => HandlePasting(e, allowedCharacters));
        }

        public static void AttachTextBoxValidation(TextBox textBox, string pattern)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException(nameof(textBox));
            }

            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Pattern cannot be null or empty");
            }

            Regex allowed;

            try
            {
                allowed = new Regex(pattern, RegexOptions.Compiled, regexTimeout);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("Invalid regex pattern");
            }

            AttachRegexValidation(textBox, allowed);
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
                try
                {
                    if (allowed.IsMatch(c.ToString()))
                    {
                        builder.Append(c);
                    }
                }
                catch (RegexMatchTimeoutException)
                {
                    /**
                     * It is ignored to avoid UI blocking: if the regular 
                     * expression exceeds the timeout (500 ms) by one 
                     * character, it is skipped to keep the application 
                     * responsive and safe, excluding dubious input 
                     * without interrupting the flow.
                     */
                }
            }
            return builder.ToString();
        }

        private static void ValidateArguments(object control, Regex regex)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            if (regex == null)
            {
                throw new ArgumentNullException(nameof(regex));
            }
        }

        private static void HandlePreviewTextInput(TextCompositionEventArgs e, Regex allowedCharacters)
        {
            try
            {
                string normalized = e.Text.Normalize(NormalizationForm.FormC);
                e.Handled = !allowedCharacters.IsMatch(normalized);
            }
            catch (ArgumentException ex)
            {
                HandleInputError(e, ex);
            }
            catch (RegexMatchTimeoutException ex)
            {
                ClientException.HandleError(ex, nameof(HandlePreviewTextInput));
                e.Handled = true;
            }
            catch (Exception ex)
            {
                HandleInputError(e, ex);
            }
        }

        private static void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private static void HandlePasting(DataObjectPastingEventArgs e, Regex allowedCharacters)
        {
            try
            {
                if (!e.DataObject.GetDataPresent(DataFormats.Text))
                {
                    e.CancelCommand();
                    return;
                }

                string pasteText = e.DataObject.GetData(DataFormats.Text) as string;

                if (string.IsNullOrEmpty(pasteText))
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
            catch (ArgumentNullException ex) 
            { 
                HandlePasteError(e, ex); 
            }
            catch (ArgumentException ex)
            {
                HandlePasteError(e, ex);
            }
            catch (RegexMatchTimeoutException ex)
            {
                ClientException.HandleError(ex, nameof(HandlePasting));
            }
            catch (ExternalException ex)
            {
                HandlePasteError(e, ex);
            }
            catch (SecurityException ex)
            {
                HandlePasteError(e, ex);
            }
            catch (Exception ex)
            {
                HandlePasteError(e, ex);
            }
        }

        private static void HandleInputError(TextCompositionEventArgs e, Exception ex)
        {
            ClientException.HandleError(ex, nameof(HandlePreviewTextInput));
            CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private static void HandlePasteError(DataObjectPastingEventArgs e, Exception ex)
        {
            ClientException.HandleError(ex, nameof(HandlePasting));
            CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            e.CancelCommand();
        }
    }
}