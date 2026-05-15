using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class JoinTournamentPage : Page
    {
        private const string MESSAGE_ERROR = "Error";

        public JoinTournamentPage()
        {
            InitializeComponent();
        }

        private async void ClickJoin(object sender, RoutedEventArgs e)
        {
            string code = txtCode.Text.Trim().ToUpper();

            if (code.Length != 6)
            {
                CustomMessageBox.Show("El código debe tener 6 caracteres.", MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SessionManager.CurrentUserData == null)
            {
                CustomMessageBox.Show("Debes iniciar sesión para unirte a un torneo.", MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnJoin.IsEnabled = false;

            try
            {
                int userId = SessionManager.CurrentUserData.PlayerId;
                bool success = await Task.Run(() => ClientManager.TournamentClient.JoinTournamentByCode(code, userId));

                if (success)
                {
                    this.NavigationService.Navigate(new LobbyTournamentPage(code, false));
                }
                else
                {
                    btnJoin.IsEnabled = true;
                    CustomMessageBox.Show("No se encontró el torneo o ya está lleno.", MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (FaultException ex)
            {
                btnJoin.IsEnabled = true;
                ClientException.HandleError(ex, nameof(ClickJoin));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                btnJoin.IsEnabled = true;
                ClientException.HandleError(ex, nameof(ClickJoin));
                CustomMessageBox.Show(Lang.ExceptionTextCommunication, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                btnJoin.IsEnabled = true;
                ClientException.HandleError(ex, nameof(ClickJoin));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred, MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new TournamentMenuPage());
        }
    }
}