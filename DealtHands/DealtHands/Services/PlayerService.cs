using DealtHands.Models;
using DealtHands.Data;
using Microsoft.EntityFrameworkCore;

namespace DealtHands.Services
{
    public class PlayerService
    {
        private readonly ApplicationDbContext _context;

        public PlayerService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Add a player to a session (or return existing player by code)
        /// </summary>
        public Player JoinSession(int sessionId, string playerName, string playerCode = null)
        {
            // If player code provided, try to find existing player
            if (!string.IsNullOrEmpty(playerCode))
            {
                var existingPlayer = _context.Players
                    .FirstOrDefault(p => p.SessionId == sessionId && p.PlayerCode == playerCode && p.IsActive);

                if (existingPlayer != null)
                {
                    return existingPlayer; // Returning player
                }
            }

            // New player - create new record
            var player = new Player
            {
                Name = playerName,
                SessionId = sessionId,
                PlayerCode = GenerateUniquePlayerCode(),
                JoinedAt = DateTime.UtcNow,
                IsActive = true,
                MonthlyIncome = 0,
                MonthlyExpenses = 0,
                TotalDebt = 0,
                Savings = 0,
                Salary = 0,
                FinancialHealth = "Healthy"
            };

            _context.Players.Add(player);
            _context.SaveChanges();

            return player;
        }

        /// <summary>
        /// Generate unique 4-digit player code
        /// </summary>
        private string GenerateUniquePlayerCode()
        {
            Random random = new Random();
            string code;

            do
            {
                code = random.Next(1000, 9999).ToString();
            }
            while (_context.Players.Any(p => p.PlayerCode == code));

            return code;
        }

        /// <summary>
        /// Get all players in a session
        /// </summary>
        public List<Player> GetPlayersInSession(int sessionId)
        {
            return _context.Players
                .Where(p => p.SessionId == sessionId && p.IsActive)
                .ToList();
        }

        /// <summary>
        /// Get a specific player
        /// </summary>
        public Player GetPlayer(int playerId)
        {
            return _context.Players
                .Include(p => p.Choices)
                .Include(p => p.GameChangersReceived)
                .FirstOrDefault(p => p.Id == playerId);
        }

        /// <summary>
        /// Update player's financial state
        /// </summary>
        public void UpdateFinancialState(int playerId, decimal income, decimal expenses, decimal debt)
        {
            var player = _context.Players.Find(playerId);
            if (player != null)
            {
                player.MonthlyIncome = income;
                player.MonthlyExpenses = expenses;
                player.TotalDebt = debt;

                // Calculate financial health
                decimal available = income - expenses;
                decimal percentageAvailable = income > 0 ? (available / income) * 100 : 0;

                if (percentageAvailable >= 30)
                    player.FinancialHealth = "Healthy";
                else if (percentageAvailable >= 10)
                    player.FinancialHealth = "Struggling";
                else
                    player.FinancialHealth = "Critical";

                _context.SaveChanges();
            }
        }
    }
}