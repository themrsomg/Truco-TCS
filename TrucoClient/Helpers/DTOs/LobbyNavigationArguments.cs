namespace TrucoClient.Helpers.DTOs
{
    public class LobbyNavigationArguments
    {
        public string MatchCode { get; set; }
        public string MatchName { get; set; }
        public int MaxPlayers { get; set; }
        public bool IsPrivate { get; set; }
    }
}
