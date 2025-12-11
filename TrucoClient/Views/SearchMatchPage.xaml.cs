using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.DTOs;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class SearchMatchPage : Page
    {
        private const string MESSAGE_ERROR = "Error";
        private const int MAX_CLICKS_ALLOWED = 5; 
        private const int TIME_WINDOW_SECONDS = 5;  
        private const int COOLDOWN_SECONDS = 10;
        private const int COOLDOWN_MILLISECONDS = 10000;
        private const int COOLDOWN_COUNTDOWN = 1000;

        private Queue<DateTime> clickTimestamps = new Queue<DateTime>();
        private bool isCooldownActive = false;

        public class MatchInfo
        {
            public string MatchName { get; set; }
            public string MatchCode { get; set; }
            public int CurrentPlayers { get; set; }
            public int MaxPlayers { get; set; }
        }

        public SearchMatchPage()
        {
            InitializeComponent();
            _ = LoadAvailableMatchesAsync();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void ClickJoinMatch(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || !(button.DataContext is MatchInfo match))
            {
                return;
            }

            try
            {
                button.IsEnabled = false;

                int result = await Task.Run(() =>
                    ClientManager.MatchClient.JoinMatch(match.MatchCode, SessionManager.CurrentUsername)
                );

                if (result > 0)
                {
                    var arguments = new LobbyNavigationArguments
                    {
                        MatchCode = match.MatchCode,
                        MatchName = match.MatchName,
                        MaxPlayers = match.MaxPlayers,
                        IsPrivate = false
                    };

                    this.NavigationService.Navigate(new LobbyPage(arguments));
                }
                else
                {
                    CustomMessageBox.Show(Lang.PreGameJoinMatchNoSuccess, MESSAGE_ERROR,
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    await LoadAvailableMatchesAsync();
                }
            }
            catch (FaultException<CustomFault> ex)
            {
                HandleJoinFault(ex);
            }
            catch (TimeoutException ex)
            {
                ClientException.HandleError(ex, nameof(ClickJoinMatch));
                CustomMessageBox.Show(Lang.ExceptionTextErrorJoiningMatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            catch (EndpointNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(ClickJoinMatch));
                CustomMessageBox.Show(Lang.ExceptionTextErrorJoiningMatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                ClientException.HandleError(ex, nameof(ClickJoinMatch));
                CustomMessageBox.Show(Lang.ExceptionTextErrorJoiningMatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(ClickJoinMatch));
                CustomMessageBox.Show(Lang.ExceptionTextErrorJoiningMatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                button.IsEnabled = true;
            }
        }

        private void HandleJoinFault(FaultException<CustomFault> ex)
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

        private async void ClickReloadPage(object sender, RoutedEventArgs e)
        {
            if (isCooldownActive)
            {
                return;
            }

            var button = sender as Button;
            var now = DateTime.Now;

            while (clickTimestamps.Count > 0 && (now - clickTimestamps.Peek()).TotalSeconds > TIME_WINDOW_SECONDS)
            {
                clickTimestamps.Dequeue();
            }

            if (clickTimestamps.Count >= MAX_CLICKS_ALLOWED)
            {
                await ActivateCooldown(button);
                return;
            }

            clickTimestamps.Enqueue(now);

            if (button != null)
            {
                button.IsEnabled = false;
            }

            await LoadAvailableMatchesAsync();

            if (button != null)
            {
                button.IsEnabled = true;
            }
        }


        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }

        private async Task ActivateCooldown(Button btn)
        {
            isCooldownActive = true;
            clickTimestamps.Clear();

            if (btn != null)
            {
                btn.IsEnabled = false;
                var originalContent = btn.Content;

                for (int i = COOLDOWN_SECONDS; i > 0; i--)
                {
                    btn.Content = $"{i}s...";
                    await Task.Delay(COOLDOWN_COUNTDOWN);
                }

                btn.Content = originalContent;
                btn.IsEnabled = true;
            }
            else
            {
                await Task.Delay(COOLDOWN_MILLISECONDS);
            }

            isCooldownActive = false;
        }

        private async Task LoadAvailableMatchesAsync()
        {
            try
            {
                var matchesArray = await Task.Run(() =>
                {
                    return ClientManager.MatchClient.GetPublicLobbies();
                });

                if (matchesArray == null)
                {
                    lstMatches.ItemsSource = null;
                    return;
                }

                var matchesList = ConvertArrayToList(matchesArray);

                if (matchesList.Count == 0)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        CustomMessageBox.Show(Lang.DialogTextNoMatches, MESSAGE_ERROR,
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }

                lstMatches.ItemsSource = matchesList.Select(m => new MatchInfo
                {
                    MatchName = m.MatchName,
                    MatchCode = m.MatchCode,
                    CurrentPlayers = m.CurrentPlayers,
                    MaxPlayers = m.MaxPlayers
                }).ToList();
            }
            catch (FaultException<CustomFault> ex)
            {
                HandleJoinFault(ex);
            }
            catch (EndpointNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(ClickJoinMatch));
                CustomMessageBox.Show(Lang.ExceptionTextErrorJoiningMatch,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                ClientException.HandleError(ex, nameof(LoadAvailableMatchesAsync));
                CustomMessageBox.Show(Lang.ExceptionTextErrorLoadingMatches,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(LoadAvailableMatchesAsync));
                CustomMessageBox.Show(Lang.ExceptionTextErrorLoadingMatches,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static List<T> ConvertArrayToList<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                return new List<T>();
            }

            return new List<T>(array);
        }
    }
}
