using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;
using TrucoClient.Helpers.UI;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Views
{
    public partial class CreateTournamentPage : Page
    {
        private const string MESSAGE_ERROR = "Error";
        private int selectedCapacity = 0;

        private static readonly SolidColorBrush BRUSH_SELECTED_BORDER =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2EF457"));
        private static readonly SolidColorBrush BRUSH_SELECTED_BG =
            new SolidColorBrush(Color.FromArgb(220, 10, 60, 10));
        private static readonly SolidColorBrush BRUSH_DEFAULT_BORDER =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555"));
        private static readonly SolidColorBrush BRUSH_DEFAULT_BG =
            new SolidColorBrush(Color.FromArgb(204, 0, 0, 0));

        public CreateTournamentPage()
        {
            InitializeComponent();
        }

        private void ClickSelect4(object sender, MouseButtonEventArgs e)
        {
            selectedCapacity = 4;
            border4.BorderBrush = BRUSH_SELECTED_BORDER;
            border4.Background = BRUSH_SELECTED_BG;
            border8.BorderBrush = BRUSH_DEFAULT_BORDER;
            border8.Background = BRUSH_DEFAULT_BG;
            btnCreate.IsEnabled = true;
        }

        private void ClickSelect8(object sender, MouseButtonEventArgs e)
        {
            selectedCapacity = 8;
            border8.BorderBrush = BRUSH_SELECTED_BORDER;
            border8.Background = BRUSH_SELECTED_BG;
            border4.BorderBrush = BRUSH_DEFAULT_BORDER;
            border4.Background = BRUSH_DEFAULT_BG;
            btnCreate.IsEnabled = true;
        }

        private async void ClickCreate(object sender, RoutedEventArgs e)
        {
            btnCreate.IsEnabled = false;

            try
            {
                int userId = SessionManager.CurrentUserData.PlayerId;
                string code = await Task.Run(() =>
                    ClientManager.TournamentClient.CreateTournament(selectedCapacity, userId));

                if (!string.IsNullOrEmpty(code))
                {
                    this.NavigationService.Navigate(new LobbyTournamentPage(code, true));
                }
                else
                {
                    CustomMessageBox.Show("No se pudo crear el torneo. Intentá de nuevo.",
                        MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                    btnCreate.IsEnabled = true;
                }
            }
            catch (FaultException ex)
            {
                ClientException.HandleError(ex, nameof(ClickCreate));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                btnCreate.IsEnabled = true;
            }
            catch (CommunicationException ex)
            {
                ClientException.HandleError(ex, nameof(ClickCreate));
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                btnCreate.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(ClickCreate));
                CustomMessageBox.Show(Lang.ExceptionTextErrorOcurred,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                btnCreate.IsEnabled = true;
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new TournamentMenuPage());
        }
    }
}