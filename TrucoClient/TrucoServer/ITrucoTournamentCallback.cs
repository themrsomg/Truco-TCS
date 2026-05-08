using System.Collections.Generic;
using TrucoClient.Helpers.DTOs;

namespace TrucoClient.TrucoServer
{
    // Interfaz local para callbacks de torneo (si falta en el proxy generado).
    public interface ITrucoTournamentCallback
    {
        void OnPlayerJoined(string username, int currentCapacity);
        void OnTournamentStarted(List<BracketDTO> initialBrackets);
        void OnBracketUpdated(BracketDTO updatedBracket);
    }
}