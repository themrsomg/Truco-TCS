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
        private const int MAX_CLICKS_ALLOWED = 5; 
        private const int TIME_WINDOW_SECONDS = 5;  
        private const int COOLDOWN_SECONDS = 10;

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
                    await Task.Delay(1000);
                }

                btn.Content = originalContent;
                btn.IsEnabled = true;
            }
            else
            {
                await Task.Delay(COOLDOWN_SECONDS * 1000);
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
