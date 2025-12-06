using System.Windows.Media.Imaging;

namespace TrucoClient.Helpers.DTOs
{
    public class FriendLobbyInfo
    {
        public string Username { get; set; }
        public BitmapImage AvatarUri { get; set; }
        public bool CanInvite { get; set; }
    }
}
