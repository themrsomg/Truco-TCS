using System.Collections.Generic;

namespace TrucoClient.TrucoServer
{
    public interface ITrucoTournamentCallback
    {
        void OnTournamentPlayerJoined(string username, int currentCapacity);
        void OnTournamentStarted(List<BracketDTO> initialBrackets);
        void OnBracketUpdated(BracketDTO updatedBracket);
    }
}