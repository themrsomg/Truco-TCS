namespace TrucoClient.Helpers.DTOs
{
    public class TournamentDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; }
    }

    public class BracketDTO
    {
        public int Id { get; set; }
        public int Round { get; set; }
        public int Position { get; set; }
        public string Player1Name { get; set; }
        public string Player2Name { get; set; }
        public string WinnerName { get; set; }
        public string MatchId { get; set; }
    }
}