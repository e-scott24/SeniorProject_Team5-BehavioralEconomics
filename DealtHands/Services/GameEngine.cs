/*  Name: Jason Black
    Date: 3/20/2026
    Last Update: 3/21/2026

    Core game logic — records player choices and applies Game Changer effects.

    RecordChoice() is called by each round page on selection.
    ApplyGameChanger() is called per card click from the overlay.
    GetRoundPage() is the single source of truth for round → page mapping.

    The correct file paths are the following:
    ...\DealtHands\DealtHands\Services\GameChangerService.cs
    ...\DealtHands\DealtHands\Pages\Shared\Cards\_GameChangerCard.cshtml
    ...\DealtHands\DealtHands\Pages\Shared\Cards\_GameChangerOverlay.cshtml
    ...\DealtHands\DealtHands\Pages\Shared\GameChangerOverlayModel.cs
    ...\DealtHands\DealtHands\Services\GameEngine.cs
*/

using DealtHands.Models;
using DealtHands.Models.Cards;

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

        // Applies a GameChangerCard's financial effect to a player.
        // Income floors at 0.
        // Deduction waterfall - balance first, then savings, then total debt.
        // DebtChange sign convention (matches BalanceChange) - negative = good (debt reduced), positive = bad (debt added).
        public void ApplyGameChanger(int playerId, GameChangerCard card, int roundNumber)
        {
            var player = _playerService.GetPlayer(playerId);
            if (player == null) return;

            // Debt adjustment (DebtCard)
            card.Apply(player);

            player.GameChangersReceived.Add(new PlayerGameChanger
            {
                PlayerId = playerId,
                GameChangerEventId = card.Id,
                RoundNumber = roundNumber,
                OccurredAt = DateTime.UtcNow
            });

            _playerService.UpdateFinancialState(
                playerId,
                player.MonthlyIncome,
                player.MonthlyExpenses,
                player.TotalDebt
            );
        }

        // RecordChoice 
        public void RecordChoice(int playerId, int roundNumber, string roundType,
                                  string choiceDescription, decimal monthlyCost,
                                  decimal? totalPrice = null, decimal? annualSalary = null)
        {
            var player = _playerService.GetPlayer(playerId);
            if (player == null) return;

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
            player.MonthlyExpenses += monthlyCost;

            if (roundNumber == 1 && totalPrice.HasValue)
            {
                player.MonthlyIncome = totalPrice.Value;
                player.Salary = annualSalary ?? 0;
            }

            _playerService.UpdateFinancialState(playerId, player.MonthlyIncome,
                                                player.MonthlyExpenses, player.TotalDebt);
        }

        public bool AreAllPlayersReady(int sessionId, int roundNumber)
        {
            var players = _playerService.GetPlayersInSession(sessionId);
            return players.All(p => p.Choices.Any(c => c.RoundNumber == roundNumber));
        }

        public void AdvanceRound(int sessionId)
        {
            var session = _sessionService.GetSessionById(sessionId);
            if (session == null) return;
            if (session.CurrentRound < 6)
                session.CurrentRound++;
        }

        public string GetRoundPage(int roundNumber)
        {
            return roundNumber switch
            {
                1 => "/Round1Career",
                2 => "/Round2Loans",
                3 => "/Round3Transportation",
                4 => "/Round4Housing",
                5 => "/Round5Family",
                _ => "/Results"
            };
        }

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
                        RequiresAmount = true
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
                    return null;
            }
        }

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
