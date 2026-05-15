using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Session;
using TrucoClient.Helpers.UI;

namespace TrucoClient.Views
{
    public partial class TournamentMenuPage : Page
    {
        public TournamentMenuPage()
        {
            InitializeComponent();
        }

        private void ClickCreate(object sender, RoutedEventArgs e)
        {
            if (SessionManager.CurrentUserData == null || SessionManager.CurrentUsername.StartsWith("Guest_"))
            {
                CustomMessageBox.Show("Debes iniciar sesión para crear un torneo.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.NavigationService.Navigate(new CreateTournamentPage());
        }

        private void ClickJoin(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new JoinTournamentPage());
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PlayPage());
        }
    }
}