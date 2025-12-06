using System.Windows.Media.Imaging;

namespace TrucoClient.Helpers.DTOs
{
    public class PlayerLobbyInfo
    {
        public string Username { get; set; }
        public BitmapImage AvatarUri { get; set; }
        public string Team { get; set; }
        public bool IsCurrentUser { get; set; }
        public string OwnerUsername { get; set; }
    }
}
