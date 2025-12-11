using System;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.DTOs;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Helpers.UI;
using TrucoClient.Helpers.Validation; 
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class JoinGamePage : Page
    {
        private const string MESSAGE_ERROR = "Error";

        private static readonly Regex codeInputRegex = new Regex(@"^[0-9a-zA-Z]*$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(500));

        public JoinGamePage()
        {
            InitializeComponent();
            InitializeValidation();
            MusicInitializer.InitializeMenuMusic();
        }

        private void InitializeValidation()
        {
            InputRestriction.AttachRegexValidation(txtCode, codeInputRegex);
        }

        private void ClickJoin(object sender, RoutedEventArgs e)
        {
            string code = txtCode.Text.Trim().ToUpperInvariant();
            string player = SessionManager.CurrentUsername;

            ErrorDisplayService.ClearError(txtCode, null);

            if (!FieldValidator.IsRequired(code))
            {
                ErrorDisplayService.ShowError(txtCode, null, Lang.GlobalTextRequieredField);
                return;
            }

            try
            {
                int maxPlayers = ClientManager.MatchClient.JoinMatch(code, player);

                if (maxPlayers > 0)
                {
                    var arguments = new LobbyNavigationArguments
                    {
                        MatchCode = code,
                        MatchName = Lang.GlobalTextPrivateMatch,
                        MaxPlayers = maxPlayers,
                        IsPrivate = true
                    };

                    this.NavigationService.Navigate(new LobbyPage(arguments));
                }
                else
                {
                    CustomMessageBox.Show(Lang.GameTextInvalidCode, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (FaultException<CustomFault> ex)
            {
                HandleJoinFault(ex);
            }
            catch (TimeoutException ex)
            {
                ClientException.HandleError(ex, nameof(ClickJoin));
                CustomMessageBox.Show(Lang.ExceptionTextErrorJoiningMatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            catch (EndpointNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(ClickJoin));
                CustomMessageBox.Show(Lang.ExceptionTextErrorJoiningMatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                ClientException.HandleError(ex, nameof(ClickJoin));
                CustomMessageBox.Show(Lang.ExceptionTextErrorJoiningMatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(ClickJoin));
                CustomMessageBox.Show(Lang.ExceptionTextErrorJoiningMatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void HandleJoinFault(FaultException<CustomFault> ex)
        {
            switch (ex.Detail.ErrorCode)
            {
                case "ServerDBErrorJoin":
                    CustomMessageBox.Show(Lang.ExceptionTextDBErrorJoin, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case "ServerDBErrorGetPublicLobbies":
                    CustomMessageBox.Show(Lang.ExceptionTextDBErrorGetPublicLobbies, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case "ServerTimeout":
                    CustomMessageBox.Show(Lang.ExceptionTextTimeout, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }

        private void CodeTextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorDisplayService.ClearError(txtCode, null);

            TextBox textBox = sender as TextBox;

            if (textBox == null)
            {
                return;
            }

            int caretIndex = textBox.CaretIndex;
            string text = textBox.Text;
            string upperText = text.ToUpperInvariant();

            if (text != upperText)
            {
                textBox.Text = upperText;
                textBox.CaretIndex = caretIndex;
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
