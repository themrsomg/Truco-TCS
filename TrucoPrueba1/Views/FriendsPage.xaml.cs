using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrucoPrueba1.TrucoServer;

namespace TrucoPrueba1
{
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
        private const string SearchPlaceholder = "Buscar nombre de usuario...";

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
            try
            {
                if (friendClient == null || friendClient.State == CommunicationState.Faulted)
                {
                    InstanceContext context = new InstanceContext(new TrucoCallbackHandler());
                    friendClient = new TrucoFriendServiceClient(context, "NetTcpBinding_ITrucoFriendService");
                    friendClient.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar el cliente de amigos: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDataAsync()
        {
            FriendsList.Clear();
            PendingList.Clear();

            string currentUsername = SessionManager.CurrentUsername;

            if (string.IsNullOrEmpty(currentUsername)) return;

            InitializeFriendClient();

            try
            {
                var friends = await friendClient.GetFriendsAsync(currentUsername);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var friend in friends)
                    {
                        FriendsList.Add(new FriendDisplayData { Username = friend.Username, AvatarId = friend.AvatarId });
                    }
                });

                var pending = await friendClient.GetPendingFriendRequestsAsync(currentUsername);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var req in pending)
                    {
                        PendingList.Add(new FriendDisplayData { Username = req.Username, AvatarId = req.AvatarId });
                    }
                });

                if (friendClient.State != CommunicationState.Closed)
                {
                    friendClient.Close();
                }
            }
            catch (Exception ex)
            {
                if (friendClient.State != CommunicationState.Closed)
                {
                    friendClient.Abort();
                }
                MessageBox.Show($"Error al cargar datos de amigos: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ClickAddFriend(object sender, RoutedEventArgs e)
        {
            string targetUsername = txtSearch.Text.Trim();
            string currentUsername = SessionManager.CurrentUsername;

            if (string.IsNullOrEmpty(targetUsername) || targetUsername == SearchPlaceholder)
            {
                MessageBox.Show("Ingresa un nombre de usuario válido.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (targetUsername.Equals(currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("No puedes enviarte una solicitud a ti mismo.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            InitializeFriendClient();
            try
            {
                bool success = await friendClient.SendFriendRequestAsync(currentUsername, targetUsername);

                if (success)
                {
                    MessageBox.Show($"Solicitud de amistad enviada a {targetUsername}.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"No se pudo enviar la solicitud. El usuario no existe, ya son amigos, o ya hay una solicitud pendiente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (friendClient.State != CommunicationState.Closed)
                {
                    friendClient.Close();
                }
            }
            catch (Exception ex)
            {
                if (friendClient.State != CommunicationState.Closed)
                {
                    friendClient.Abort();
                }
                MessageBox.Show($"Error de conexión al enviar solicitud: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void ClickAcceptRequest(object sender, RoutedEventArgs e)
        {
            string requesterUsername = (sender as Button)?.Tag?.ToString();
            if (string.IsNullOrEmpty(requesterUsername)) return;

            InitializeFriendClient();
            try
            {
                bool success = await friendClient.AcceptFriendRequestAsync(requesterUsername, SessionManager.CurrentUsername);

                if (success)
                {
                    MessageBox.Show($"Has aceptado la solicitud de {requesterUsername}. Ahora son amigos.", "Solicitud Aceptada", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("No se pudo aceptar la solicitud.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (friendClient.State != CommunicationState.Closed)
                {
                    friendClient.Close();
                }
            }
            catch (Exception ex)
            {
                if (friendClient.State != CommunicationState.Closed)
                {
                    friendClient.Abort();
                }
                MessageBox.Show($"Error de conexión al aceptar solicitud: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ClickRejectRequest(object sender, RoutedEventArgs e)
        {
            string targetUsername = (sender as Button)?.Tag?.ToString();
            if (string.IsNullOrEmpty(targetUsername)) return;

            InitializeFriendClient();
            try
            {
                // Este método maneja tanto el rechazo de solicitudes como la eliminación de amigos
                bool success = await friendClient.RemoveFriendOrRequestAsync(targetUsername, SessionManager.CurrentUsername);

                if (success)
                {
                    MessageBox.Show($"Se ha completado la acción con {targetUsername}.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("No se pudo completar la acción.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (friendClient.State != CommunicationState.Closed)
                {
                    friendClient.Close();
                }
            }
            catch (Exception ex)
            {
                if (friendClient.State != CommunicationState.Closed)
                {
                    friendClient.Abort();
                }
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == SearchPlaceholder)
                txtSearch.Text = string.Empty;
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
                txtSearch.Text = SearchPlaceholder;
        }
    }
}
