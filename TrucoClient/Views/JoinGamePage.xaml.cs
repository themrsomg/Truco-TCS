using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class JoinGamePage : Page
    {
        private const string MESSAGE_ERROR = "Error";
        private static readonly Regex allowedCodeRegex = new Regex("^[A-Z0-9]+$", RegexOptions.Compiled);

        public JoinGamePage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickJoin(object sender, RoutedEventArgs e)
        {
            string code = txtCode.Text.Trim();
            string player = SessionManager.CurrentUsername;

            try
            {
                bool joined = ClientManager.MatchClient.JoinMatch(code, player);
               
                if (joined)
                {
                    this.NavigationService.Navigate(new LobbyPage(code, Lang.GlobalTextPrivateMatch));
                }
                else
                {
                    CustomMessageBox.Show(Lang.GameTextInvalidCode, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.GameTextErrorJoining, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }

        private void CodePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                e.Handled = !allowedCodeRegex.IsMatch(e.Text.ToUpperInvariant());
            }
            catch (ArgumentException ex)
            {
                CustomMessageBox.Show(ex.Message, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
        }

        private void CodePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Tab)
            {
                e.Handled = true;
            }
        }

        private void CodeTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                int caret = textBox.CaretIndex;

                string sanitized = InputSanitizer.SanitizeForCodeInput(textBox.Text).ToUpperInvariant();
                sanitized = sanitized.ToUpperInvariant();
                sanitized = InputRestriction.RestrictToAllowedCharacters(sanitized, allowedCodeRegex);

                if (textBox.Text != sanitized)
                {
                    textBox.Text = sanitized;
                    textBox.CaretIndex = Math.Min(caret, textBox.Text.Length);
                }
            }
            catch (NullReferenceException ex)
            {
                CustomMessageBox.Show(ex.Message, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CodePasting(object sender, DataObjectPastingEventArgs e)
        {
            try
            {
                if (!e.DataObject.GetDataPresent(DataFormats.Text))
                {
                    e.CancelCommand();
                    return;
                }

                string text = (string)e.DataObject.GetData(DataFormats.Text);

                text = InputSanitizer.SanitizeForCodeInput(text);
                text = text.ToUpperInvariant();
                text = InputRestriction.RestrictToAllowedCharacters(text, allowedCodeRegex);

                if (string.IsNullOrWhiteSpace(text))
                {
                    e.CancelCommand();
                }
            }
            catch (FormatException ex)
            {
                CustomMessageBox.Show(ex.Message, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                e.CancelCommand();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                e.CancelCommand();
            }
        }


        private void EnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender == txtCode)
            {
                ClickJoin(btnJoin, null);
                e.Handled = true;
            }
        }
    }
}
