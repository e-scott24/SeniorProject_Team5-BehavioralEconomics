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
                                  string choiceDescription, decimal monthlyCost, decimal? totalPrice = null, decimal? annualSalary = null)
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
                player.Salary = annualSalary ?? 0; // Store annual salary for reference
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

            return players.All(player => player.Choices.Any(c => c.RoundNumber == roundNumber));

            /*
            foreach (var player in players)
            {
                // Check if player has made a choice for this round
                bool hasChoice = player.Choices.Any(c => c.RoundNumber == roundNumber);
                if (!hasChoice) return false;
            }

            return true;
            */
        }


        /// Advance game to the next round
        public void AdvanceRound(int sessionId)
        {
            var session = _sessionService.GetSessionById(sessionId);
            if (session == null) return;
            if (session.CurrentRound < 6)
                session.CurrentRound++;
            //_sessionService.UpdateSession(session);
        }


        /// Determine the Razor Page for a round
        public string GetRoundPage(int roundNumber)
        {

            return "/Round";

            /*
            return roundNumber switch
            {
                1 => "/Round1Career",
                2 => "/Round2Loans",
                3 => "/Round3Transportation",
                4 => "/Round4Housing",
                5 => "/Round5Family",
                _ => "/Results"
            };
            */
        }


        // Get round configuration (for dynamic round handling)
        public RoundConfig GetRoundConfig(int roundNumber)
        {
            switch (roundNumber)
            {
                case 1:
                    return new RoundConfig
                    {
                        RoundNumber = 1,
                        RoundName = "Career",
                        RoundType = "Career",
                        Choices = new List<string> { "Teacher", "Engineer", "Doctor", "Artist" },
                        RequiresAmount = true // salary input
                    };
                case 2:
                    return new RoundConfig
                    {
                        RoundNumber = 2,
                        RoundName = "Loans",
                        RoundType = "Loans",
                        Choices = new List<string> { "Bank Loan", "Private Loan", "No Loan" },
                        RequiresAmount = true
                    };
                case 3:
                    return new RoundConfig
                    {
                        RoundNumber = 3,
                        RoundName = "Transportation",
                        RoundType = "Transportation",
                        Choices = new List<string> { "Car", "Bike", "Public Transit" },
                        RequiresAmount = true
                    };
                case 4:
                    return new RoundConfig
                    {
                        RoundNumber = 4,
                        RoundName = "Housing",
                        RoundType = "Housing",
                        Choices = new List<string> { "Rent", "Buy Condo", "Buy House" },
                        RequiresAmount = true
                    };
                case 5:
                    return new RoundConfig
                    {
                        RoundNumber = 5,
                        RoundName = "Family",
                        RoundType = "Family",
                        Choices = new List<string> { "Single", "Married", "Married with Kids" },
                        RequiresAmount = false
                    };
                default:
                    return null; // No more rounds
            }
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

    // Helper class for displaying results
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