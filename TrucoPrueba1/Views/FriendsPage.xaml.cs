using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrucoPrueba1.TrucoServer;

namespace TrucoPrueba1
{
    /// <summary>
    /// Lógica de interacción para FriendsPage.xaml
    /// </summary>
    /// 
    public class FriendDisplayData
    {
        public string Username { get; set; }
        public string AvatarId { get; set; }
    }
    public partial class FriendsPage : Page
    {
        public ObservableCollection<FriendDisplayData> FriendsList { get; set; } = new ObservableCollection<FriendDisplayData>();
        public ObservableCollection<FriendDisplayData> PendingList { get; set; } = new ObservableCollection<FriendDisplayData>();
        public FriendDisplayData SelectedFriend { get; set; }
        public FriendDisplayData SelectedPending { get; set; }
        private TrucoFriendServiceClient friendClient;
        public FriendsPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
            this.DataContext = this;
            InitializeFriendClient();
            _ = LoadDataAsync();
        }
        private void InitializeFriendClient()
        {
            if (friendClient == null || friendClient.State == CommunicationState.Faulted)
            {
                try
                {
                    InstanceContext userContext = new InstanceContext(new TrucoCallbackHandler());
                    friendClient = new TrucoFriendServiceClient(userContext, "NetTcpBinding_ITrucoFriendService");
                    if (friendClient.State == CommunicationState.Created)
                    {
                        friendClient.Open();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error inicializando el cliente de amigos: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async Task LoadDataAsync()
        {
            InitializeFriendClient();

            if (friendClient == null || friendClient.State != CommunicationState.Opened)
            {
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                var friendsData = await friendClient.GetAcceptedFriendsDataAsync(SessionManager.CurrentUsername);
                var pendingData = await friendClient.GetPendingFriendRequestsDataAsync(SessionManager.CurrentUsername);
                FriendsList.Clear();
                if (friendsData != null)
                {
                    foreach (var friend in friendsData)
                    {
                        FriendsList.Add(new FriendDisplayData
                        {
                            Username = friend.Username,
                            AvatarId = friend.AvatarId
                        });
                    }
                }

                PendingList.Clear();
                if (pendingData != null)
                {
                    foreach (var request in pendingData)
                    {
                        PendingList.Add(new FriendDisplayData
                        {
                            Username = request.Username,
                            AvatarId = request.AvatarId
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando la lista de amigos: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        private async void ClickAddFriend(object sender, RoutedEventArgs e)
        {
            string friendUsername = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(friendUsername) || friendUsername == SessionManager.CurrentUsername || friendUsername == "Buscar nombre de usuario...")
            {
                MessageBox.Show("Ingresa un nombre de usuario válido.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            InitializeFriendClient();

            try
            {
                bool success = await friendClient.SendFriendRequestAsync(SessionManager.CurrentUsername, friendUsername);
                if (success)
                {
                    MessageBox.Show($"Solicitud de amistad enviada a {friendUsername}.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtSearch.Text = "Buscar nombre de usuario...";
                    txtSearch.LostFocus -= TxtSearch_LostFocus;
                    txtSearch.LostFocus += TxtSearch_LostFocus;
                }
                else
                {
                    MessageBox.Show($"No se pudo enviar la solicitud. El usuario ya existe o hay un problema en el servidor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión al enviar solicitud: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ClickDeleteFriend(object sender, RoutedEventArgs e)
        {
            if (SelectedFriend == null)
            {
                MessageBox.Show("Selecciona un amigo de la lista para eliminar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"¿Estás seguro de que quieres eliminar a {SelectedFriend.Username} de tus amigos?", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                InitializeFriendClient();

                try
                {
                    bool success = await friendClient.RemoveFriendOrRequestAsync(SessionManager.CurrentUsername, SelectedFriend.Username);

                    if (success)
                    {
                        MessageBox.Show($"{SelectedFriend.Username} ha sido eliminado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDataAsync(); 
                    }
                    else
                    {
                        MessageBox.Show("No se pudo eliminar el amigo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error de conexión al eliminar amigo: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ClickAcceptRequest(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string requesterUsername = btn?.Tag as string;

            if (string.IsNullOrEmpty(requesterUsername)) return;

            InitializeFriendClient();

            try
            {
                await friendClient.AcceptFriendRequestAsync(requesterUsername, SessionManager.CurrentUsername);

                MessageBox.Show($"Has aceptado la solicitud de {requesterUsername}.", "Solicitud Aceptada", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión al aceptar solicitud: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ClickRejectRequest(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string requesterUsername = btn?.Tag as string;

            if (string.IsNullOrEmpty(requesterUsername)) return;

            InitializeFriendClient();

            try
            {
                bool success = await friendClient.RemoveFriendOrRequestAsync(requesterUsername, SessionManager.CurrentUsername);

                if (success)
                {
                    MessageBox.Show($"Has rechazado la solicitud de {requesterUsername}.", "Solicitud Rechazada", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync(); 
                }
                else
                {
                    MessageBox.Show("No se pudo rechazar la solicitud.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión al rechazar solicitud: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }
        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "Buscar nombre de usuario...")
            {
                txtSearch.Text = string.Empty;
            }
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Buscar nombre de usuario...";
            }
        }
    }
}
