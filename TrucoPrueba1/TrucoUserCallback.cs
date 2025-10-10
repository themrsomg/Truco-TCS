using System;
using TrucoPrueba1.TrucoServer;

public class TrucoUserCallback : ITrucoUserServiceCallback
{
    public void OnPlayerJoined(string matchCode, string player) { }
    public void OnPlayerLeft(string matchCode, string player) { }
    public void OnCardPlayed(string matchCode, string player, string card) { }
    public void OnChatMessage(string matchCode, string player, string message) { }
    public void OnMatchStarted(string matchCode) { }
    public void OnMatchEnded(string matchCode, string winner) { }
    public void OnFriendRequestReceived(string fromUser) { }
    public void OnFriendRequestAccepted(string fromUser) { }
}
