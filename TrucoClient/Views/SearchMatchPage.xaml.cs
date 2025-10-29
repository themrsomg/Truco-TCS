using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
            LoadAvailableMatches();
        }

        private void LoadAvailableMatches()
        {
            var matches = new List<MatchInfo>
            {
                new MatchInfo { MatchName = "Partida 1 - 1v1", MatchCode = "TRU001", CurrentPlayers = 1, MaxPlayers = 2 },
                new MatchInfo { MatchName = "Partida 2 - 1v1", MatchCode = "TRU002", CurrentPlayers = 1, MaxPlayers = 2 },
                new MatchInfo { MatchName = "Partida 3 - 2v2", MatchCode = "TRU003", CurrentPlayers = 3, MaxPlayers = 4 }
            };

            lstMatches.ItemsSource = matches;
        }

        private void ClickJoinMatch(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is MatchInfo match)
            {
                this.NavigationService.Navigate(new LobbyPage(match.MatchCode, match.MatchName));
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }
    }
}
