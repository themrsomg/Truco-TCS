using System.Collections.Generic;

namespace TrucoClient.TrucoServer
{
    public interface ITrucoTournamentCallback
    {
        void OnTournamentPlayerJoined(string username, int currentCapacity);
        void OnTournamentPlayerLeft(string username, int currentCapacity);
        void OnTournamentStarted(List<BracketDTO> initialBrackets);
        void OnTournamentCancelled(string reason);
        void OnBracketUpdated(BracketDTO updatedBracket);
    }
}