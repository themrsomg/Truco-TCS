using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TrucoPrueba1.TrucoServer;

namespace TrucoPrueba1.TrucoServer
{
    public class TrucoCallbackHandler : ITrucoUserServiceCallback, ITrucoFriendServiceCallback, ITrucoMatchServiceCallback
    {
        public void OnPlayerJoined(string matchCode, string player) { }
        public void OnPlayerLeft(string matchCode, string player) { }
        public void OnCardPlayed(string matchCode, string player, string card) { }
        public void OnChatMessage(string matchCode, string player, string message) { }
        public void OnMatchStarted(string matchCode) { }
        public void OnMatchEnded(string matchCode, string winner) { }

        public void OnFriendRequestReceived(string fromUser)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Tienes una nueva solicitud de amistad de {fromUser}.", "Nueva Solicitud", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public void OnFriendRequestAccepted(string fromUser)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"¡{fromUser} aceptó tu solicitud de amistad!", "Amistad Aceptada", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
        public void MatchFound(string matchDetails)
        {
            // Lógica para cuando se encuentra una partida
        }

        public void PlayerJoined(string username)
        {
            // Lógica para cuando un jugador se une
        }
    }
}
