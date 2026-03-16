
namespace DealtHands.Models
{
    public class Session
    {
        // Primary key for database
        public int Id { get; set; }

        // The 5-digit code students use to join
        public string Code { get; set; }

        // "Period 3 - Economics"
        public string Name { get; set; }

        // "Random" or "Structured"
        public string GameMode { get; set; }

        // "Easy", "Medium", or "Hard"
        public string Difficulty { get; set; }

        // 35, for example
        public int MaxPlayers { get; set; }

        // When was this session created?
        public DateTime CreatedAt { get; set; }

        // Has the game started yet?
        public bool IsStarted { get; set; }

        // Is the game finished?
        public bool IsCompleted { get; set; }

        // Which round are we on? (1-5)
        public int CurrentRound { get; set; } = 1; //default to round 1 when session is created

        // ===== RELATIONSHIPS =====
        // A session has many players
        public List<Player> Players { get; set; } = new List<Player>();

        // A session has many game changers that occurred
        public List<GameChangerEvent> GameChangers { get; set; } = new List<GameChangerEvent>();
    }
}