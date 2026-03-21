using DealtHands.Models;
using DealtHands.Data;
using Microsoft.EntityFrameworkCore;

namespace DealtHands.Services
{
    public class SessionService
    {
        private readonly ApplicationDbContext _context;

        public SessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new game session
        /// </summary>
        public Session CreateSession(string name, string gameMode, string difficulty, int maxPlayers, int? educatorId = null)
        {
            var session = new Session
            {
                Code = GenerateUniqueCode(),
                Name = name,
                GameMode = gameMode,
                Difficulty = difficulty,
                MaxPlayers = maxPlayers,
                EducatorId = educatorId,
                CreatedAt = DateTime.UtcNow,
                IsStarted = false,
                IsCompleted = false,
                CurrentRound = 1
            };

            _context.Sessions.Add(session);
            _context.SaveChanges();

            return session;
        }

        /// <summary>
        /// Get session by ID
        /// </summary>
        public Session GetSessionById(int sessionId)
        {
            return _context.Sessions
                .Include(s => s.Players)
                .FirstOrDefault(s => s.Id == sessionId);
        }

        /// <summary>
        /// Find a session by its code
        /// </summary>
        public Session GetSessionByCode(string code)
        {
            return _context.Sessions
                .Include(s => s.Players)
                .FirstOrDefault(s => s.Code == code);
        }

        /// <summary>
        /// Start the game (advance to Round 1)
        /// </summary>
        public void StartSession(int sessionId)
        {
            var session = _context.Sessions.Find(sessionId);
            if (session != null)
            {
                session.IsStarted = true;
                session.CurrentRound = 1;
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Get all sessions (for debugging purposes)
        /// </summary>
        public List<Session> GetAllSessions()
        {
            return _context.Sessions.ToList();
        }

        /// <summary>
        /// Advance to next round
        /// </summary>
        public void AdvanceToNextRound(int sessionId)
        {
            var session = _context.Sessions.Find(sessionId);
            if (session != null && session.CurrentRound < 5)
            {
                session.CurrentRound++;
                _context.SaveChanges();
            }
            else if (session != null && session.CurrentRound == 5)
            {
                session.IsCompleted = true;
                _context.SaveChanges();
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
            while (_context.Sessions.Any(s => s.Code == code));

            return code;
        }

        /// <summary>
        /// Cancel a session and mark all players as inactive
        /// </summary>
        public void CancelSession(int sessionId)
        {
            var session = _context.Sessions
                .Include(s => s.Players)
                .FirstOrDefault(s => s.Id == sessionId);

            if (session != null)
            {
                session.IsCompleted = true;
                session.IsActive = false;

                // Mark all players as inactive
                foreach (var player in session.Players)
                {
                    player.IsActive = false;
                }

                _context.SaveChanges();
            }
        }


    }
}