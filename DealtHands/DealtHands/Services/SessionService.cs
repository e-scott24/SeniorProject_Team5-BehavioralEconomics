using DealtHands.Models;

namespace DealtHands.Services
{
    public class SessionService
    {
        // For now, in-memory storage (replace with database later)
        private static List<Session> _sessions = new List<Session>();
        private static int _nextId = 1;

        /// <summary>
        /// Create a new game session
        /// </summary>
        public Session CreateSession(string name, string gameMode, string difficulty, int maxPlayers)
        {
            var session = new Session
            {
                Id = _nextId++,
                Code = GenerateUniqueCode(),
                Name = name,
                GameMode = gameMode,
                Difficulty = difficulty,
                MaxPlayers = maxPlayers,
                CreatedAt = DateTime.UtcNow,
                IsStarted = false,
                IsCompleted = false,
                CurrentRound = 0
            };

            _sessions.Add(session);
            return session;
        }

        /// <summary>
        /// Find a session by its code
        /// </summary>
        public Session GetSessionByCode(string code)
        {
            return _sessions.FirstOrDefault(s => s.Code == code);
        }

        /// <summary>
        /// Start the game (advance to Round 1)
        /// </summary>
        public void StartSession(int sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                session.IsStarted = true;
                session.CurrentRound = 1;
            }
        }

        /// <summary>
        /// Advance to next round
        /// </summary>
        public void AdvanceToNextRound(int sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null && session.CurrentRound < 5)
            {
                session.CurrentRound++;
            }
            else if (session != null && session.CurrentRound == 5)
            {
                session.IsCompleted = true;
            }
        }

        /// <summary>
        /// Generate a unique 5-digit session code
        /// </summary>
        private string GenerateUniqueCode()
        {
            Random random = new Random();
            string code;

            do
            {
                code = random.Next(10000, 99999).ToString();
            }
            while (_sessions.Any(s => s.Code == code));

            return code;
        }
    }
}