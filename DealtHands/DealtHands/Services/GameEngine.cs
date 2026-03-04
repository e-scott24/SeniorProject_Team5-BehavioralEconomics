using DealtHands.Models;

namespace DealtHands.Services
{
    public class GameEngine
    {
        private readonly SessionService _sessionService;
        private readonly PlayerService _playerService;

        public GameEngine(SessionService sessionService, PlayerService playerService)
        {
            _sessionService = sessionService;
            _playerService = playerService;
        }

        /// <summary>
        /// Record a player's choice in a round
        /// </summary>
        public void RecordChoice(int playerId, int roundNumber, string roundType,
                                  string choiceDescription, decimal monthlyCost, decimal? totalPrice = null)
        {
            var player = _playerService.GetPlayer(playerId);
            if (player == null) return;

            // Create the choice record
            var choice = new PlayerChoice
            {
                PlayerId = playerId,
                RoundNumber = roundNumber,
                RoundType = roundType,
                ChoiceDescription = choiceDescription,
                MonthlyCost = monthlyCost,
                TotalPrice = totalPrice,
                ChosenAt = DateTime.UtcNow
            };

            player.Choices.Add(choice);

            // Update player's cumulative expenses
            player.MonthlyExpenses += monthlyCost;

            // If it's Round 1 (Career), set income
            if (roundNumber == 1 && totalPrice.HasValue)
            {
                player.MonthlyIncome = totalPrice.Value; // Career sets monthly income
            }

            // Recalculate financial health
            _playerService.UpdateFinancialState(playerId, player.MonthlyIncome,
                                                player.MonthlyExpenses, player.TotalDebt);
        }

        /// <summary>
        /// Check if all players in a session have completed the current round
        /// </summary>
        public bool AreAllPlayersReady(int sessionId, int roundNumber)
        {
            var players = _playerService.GetPlayersInSession(sessionId);

            foreach (var player in players)
            {
                // Check if player has made a choice for this round
                bool hasChoice = player.Choices.Any(c => c.RoundNumber == roundNumber);
                if (!hasChoice) return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate final results for a player
        /// </summary>
        public PlayerResults GetPlayerResults(int playerId)
        {
            var player = _playerService.GetPlayer(playerId);
            if (player == null) return null;

            return new PlayerResults
            {
                PlayerName = player.Name,
                MonthlyIncome = player.MonthlyIncome,
                MonthlyExpenses = player.MonthlyExpenses,
                MonthlyAvailable = player.MonthlyIncome - player.MonthlyExpenses,
                TotalDebt = player.TotalDebt,
                Savings = player.Savings,
                FinancialHealth = player.FinancialHealth,
                ChoicesMade = player.Choices.Count,
                GameChangersHit = player.GameChangersReceived.Count
            };
        }
    }

    // Helper class for results
    public class PlayerResults
    {
        public string PlayerName { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyExpenses { get; set; }
        public decimal MonthlyAvailable { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal Savings { get; set; }
        public string FinancialHealth { get; set; }
        public int ChoicesMade { get; set; }
        public int GameChangersHit { get; set; }
    }
}