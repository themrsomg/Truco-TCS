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
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(500);

        public static void AttachRegexValidation(TextBox textBox, Regex allowedCharacters)
        {
            ValidateArguments(textBox, allowedCharacters);

            textBox.PreviewTextInput += (s, e) => HandlePreviewTextInput(e, allowedCharacters);
            textBox.PreviewKeyDown += HandlePreviewKeyDown;
            DataObject.AddPastingHandler(textBox, (s, e) => HandlePasting(e, allowedCharacters));
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
                allowed = new Regex(pattern, RegexOptions.Compiled, RegexTimeout);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("Invalid regex pattern: " + ex.Message, MESSAGE_ERROR);
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
                    /*
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
            catch (ArgumentException)
            {
                HandleInputError(e);
            }
            catch (RegexMatchTimeoutException)
            {
                ClientException.HandleError(ex, nameof(HandlePreviewTextInput));
                e.Handled = true;
            }
            catch (Exception)
            {
                HandleInputError(e);
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
            catch (ArgumentNullException) 
            { 
                HandlePasteError(e); 
            }
            catch (ArgumentException) 
            { 
                HandlePasteError(e); 
            }
            catch (RegexMatchTimeoutException ex) 
            {
                ClientException.HandleError(ex, nameof(HandlePasting));
            }
            catch (ExternalException) 
            { 
                HandlePasteError(e); 
            }
            catch (SecurityException) 
            { 
                HandlePasteError(e); 
            }
            catch (Exception) 
            { 
                HandlePasteError(e); 
            }
        }

        private static void HandleInputError(TextCompositionEventArgs e)
        {
            CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private static void HandlePasteError(DataObjectPastingEventArgs e)
        {
            CustomMessageBox.Show(Lang.ExceptionTextDataReadingError, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            e.CancelCommand();
        }
    }
}