using DealtHands.Models;

namespace DealtHands.Services
{
    public class PlayerService
    {
        private static List<Player> _players = new List<Player>();
        private static int _nextId = 1;

        /// <summary>
        /// Add a player to a session
        /// </summary>
        public Player JoinSession(int sessionId, string playerName)
        {
            var player = new Player
            {
                Id = _nextId++,
                Name = playerName,
                SessionId = sessionId,
                JoinedAt = DateTime.UtcNow,
                IsActive = true,
                MonthlyIncome = 0,
                MonthlyExpenses = 0,
                TotalDebt = 0,
                Savings = 0,
                FinancialHealth = "Healthy"
            };

            _players.Add(player);
            return player;
        }

        /// <summary>
        /// Get all players in a session
        /// </summary>
        public List<Player> GetPlayersInSession(int sessionId)
        {
            return _players.Where(p => p.SessionId == sessionId && p.IsActive).ToList();
        }

        /// <summary>
        /// Get a specific player
        /// </summary>
        public Player GetPlayer(int playerId)
        {
            return _players.FirstOrDefault(p => p.Id == playerId);
        }

        /// <summary>
        /// Update player's financial state
        /// </summary>
        public void UpdateFinancialState(int playerId, decimal income, decimal expenses, decimal debt)
        {
            var player = _players.FirstOrDefault(p => p.Id == playerId);
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
            }
        }
    }
}