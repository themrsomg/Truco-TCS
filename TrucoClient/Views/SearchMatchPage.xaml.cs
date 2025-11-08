using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class SearchMatchPage : Page
    {
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
            LoadAvailableMatchesAsync();
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
                        MessageBox.Show(Lang.PreGameJoinMatchNoSuccess, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadAvailableMatchesAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Lang.ExceptionTextErrorJoiningMatch, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void ClickReloadPage(object sender, RoutedEventArgs e)
        {
            LoadAvailableMatchesAsync();
        }


        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }

        private async void LoadAvailableMatchesAsync()
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
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorLoadingMatches, ex.Message),
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
