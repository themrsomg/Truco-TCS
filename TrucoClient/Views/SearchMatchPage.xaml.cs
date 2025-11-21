using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;
using TrucoClient.Properties.Langs;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;

namespace TrucoClient.Views
{
    public partial class SearchMatchPage : Page
    {
        private const string MESSAGE_ERROR = "Error";
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
            if (sender is Button button && button.DataContext is MatchInfo match)
            {
                bool joined = false;

                try
                {
                    if (button != null) 
                    { 
                        button.IsEnabled = false; 
                    }

                    joined = await Task.Run(() =>
                        ClientManager.MatchClient.JoinMatch(match.MatchCode, SessionManager.CurrentUsername)
                    );

                    if (joined)
                    {
                        this.NavigationService.Navigate(new LobbyPage(match.MatchCode, match.MatchName));
                    }
                    else
                    {
                        CustomMessageBox.Show(Lang.PreGameJoinMatchNoSuccess, MESSAGE_ERROR, 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        await LoadAvailableMatchesAsync();
                    }
                }
                catch (Exception)
                {
                    CustomMessageBox.Show(Lang.ExceptionTextErrorJoiningMatch, MESSAGE_ERROR, 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (button != null) 
                    { 
                        button.IsEnabled = true; 
                    }
                }
            }
        }

        private async void ClickReloadPage(object sender, RoutedEventArgs e)
        {
            await LoadAvailableMatchesAsync();
        }


        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }

        private async Task LoadAvailableMatchesAsync()
        {
            try
            {
                var matchesArray = await Task.Run(() =>
                {
                    return ClientManager.MatchClient.GetPublicLobbies();
                });

                var matchesList = ConvertArrayToList(matchesArray);

                lstMatches.ItemsSource = matchesList.Select(m => new MatchInfo
                {
                    MatchName = m.MatchName,
                    MatchCode = m.MatchCode,
                    CurrentPlayers = m.CurrentPlayers,
                    MaxPlayers = m.MaxPlayers
                }).ToList();
            }
            catch (Exception)
            {
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
