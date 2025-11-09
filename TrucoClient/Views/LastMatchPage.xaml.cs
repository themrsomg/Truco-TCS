using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Audio;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class LastMatchPage : Page
    {
        public LastMatchPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
            _ = LoadLastMatchesAsync();
        }

        private async Task LoadLastMatchesAsync()
        {
            string username = SessionManager.CurrentUsername;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show(Lang.GlobalTextSessionError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.NavigationService.Navigate(new LogInPage());
                return;
            }

            try
            {
                var userClient = ClientManager.UserClient;
                var matches = await userClient.GetLastMatchesAsync(username);

                if (matches != null && matches.Any())
                {
                    var displayData = matches.Select(m => new
                    {
                        MatchID = $"#{m.MatchID}",
                        Result = m.IsWin ? Lang.GlobalTextWinner : Lang.GlobalTextLoser,
                        Date = m.EndedAt.ToString("dd/MM/yyyy HH:mm"),
                        FinalScore = m.FinalScore
                    }).ToList();
                    dgRankings.ItemsSource = displayData;
                }
                else
                {
                    MessageBox.Show(Lang.DialogTextNoMatches, Lang.GlobalTextInformation, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (EndpointNotFoundException ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextConnectionError, ex.Message), Lang.GlobalTextConnectionError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.ExceptionTextErrorOcurred, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RankingsPage());
        }
    }
}
