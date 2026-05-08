using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.DTOs;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class JoinTournamentPage : Page
    {
        private const string MESSAGE_ERROR = "Error";
        private const string CAPACITY_FORMAT = "0/{0}";

        public JoinTournamentPage()
        {
            InitializeComponent();
            this.Loaded += PageLoaded;
        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PageLoaded;
            await LoadTournamentsAsync();
        }

        private async Task LoadTournamentsAsync()
        {
            try
            {
                var serverTournaments = await FetchTournaments();
                UpdateTournamentsUI(serverTournaments);
            }
            catch (FaultException ex)
            {
                ClientException.HandleError(ex, nameof(LoadTournamentsAsync));
                ShowErrorDialog();
            }
            catch (CommunicationException ex)
            {
                ClientException.HandleError(ex, nameof(LoadTournamentsAsync));
                ShowErrorDialog();
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(LoadTournamentsAsync));
                ShowErrorDialog();
            }
        }

        private async Task<List<TournamentDTO>> FetchTournaments()
        {
            return await Task.Run(() => ClientManager.TournamentClient.GetAvailableTournaments());
        }

        private void UpdateTournamentsUI(List<TournamentDTO> tournaments)
        {
            var displayList = new List<object>();

            foreach (var tournament in tournaments)
            {
                var displayItem = new
                {
                    Id = tournament.Id,
                    Name = tournament.Name,
                    CapacityText = string.Format(CAPACITY_FORMAT, tournament.Capacity)
                };

                displayList.Add(displayItem);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                TournamentsList.ItemsSource = displayList;
            });
        }

        private void ShowErrorDialog()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        private async void ClickJoinTournament(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            if (!(button.CommandParameter is int tournamentId))
            {
                return;
            }

            button.IsEnabled = false;

            await AttemptJoinTournament(tournamentId, button);
        }

        private async Task AttemptJoinTournament(int tournamentId, Button button)
        {
            try
            {
                int userId = SessionManager.CurrentUserData.PlayerID;
                bool success = await Task.Run(() => ClientManager.TournamentClient.SubscribeToTournament(tournamentId, userId));

                ProcessJoinResult(success, tournamentId, button);
            }
            catch (FaultException ex)
            {
                HandleJoinError(ex, button);
            }
            catch (CommunicationException ex)
            {
                HandleJoinError(ex, button);
            }
            catch (Exception ex)
            {
                HandleJoinError(ex, button);
            }
        }

        private void ProcessJoinResult(bool success, int tournamentId, Button button)
        {
            if (success)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.NavigationService.Navigate(new LobbyTournamentPage(tournamentId));
                });
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    button.IsEnabled = true;
                    CustomMessageBox.Show(Lang.ExceptionTextErrorJoiningMatch, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
        }

        private void HandleJoinError(Exception ex, Button button)
        {
            ClientException.HandleError(ex, nameof(AttemptJoinTournament));

            Application.Current.Dispatcher.Invoke(() =>
            {
                button.IsEnabled = true;
                ShowErrorDialog();
            });
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }
    }
}