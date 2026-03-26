using DealtHands.Data;
using DealtHands.ModelsV2;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DealtHands.Services
{
    /// <summary>
    /// Service for managing game sessions, rounds, and card interactions in the V2 schema.
    /// Handles the complete lifecycle from session creation to completion.
    /// </summary>
    public class GameSessionService
    {
        private readonly DealtHandsDbv2Context _context;
        private readonly Random _random;

        // Constants for the DealtHands game
        private const long DEALTHANDS_GAME_ID = 1; // The Game.GameId for DealtHands
        private static readonly string[] ROUND_TYPES = new[]
        {
            "Career",           // Round 1
            "StudentLoan",      // Round 2
            "Transportation",   // Round 3
            "Housing",          // Round 4
            "Family"            // Round 5
        };

        public GameSessionService(DealtHandsDbv2Context context)
        {
            _context = context;
            _random = new Random();
        }

        #region Session Management

        /// <summary>
        /// Creates a new game session hosted by the specified educator/user.
        /// </summary>
        /// <param name="hostUserId">The UserId of the educator hosting this session</param>
        /// <param name="sessionName">Optional friendly name for the session</param>
        /// <param name="difficulty">"Easy", "Medium", or "Hard" — affects game changer probability</param>
        /// <returns>The created GameSession with a unique JoinCode</returns>
        public async Task<GameSession> CreateSessionAsync(long hostUserId, string? sessionName = null, string? difficulty = null)
        {
            string joinCode;
            do
            {
                joinCode = GenerateJoinCode();
            }
            while (await _context.GameSessions.AnyAsync(s => s.JoinCode == joinCode && s.IsActive));

            var session = new GameSession
            {
                GameId = DEALTHANDS_GAME_ID,
                HostUserId = hostUserId,
                JoinCode = joinCode,
                Name = sessionName,
                Difficulty = difficulty,
                Status = "Waiting",
                CurrentRoundNumber = 1,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.GameSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        /// <summary>
        /// Starts a game session, transitioning it from Waiting to InProgress.
        /// </summary>
        public async Task<bool> StartSessionAsync(long gameSessionId)
        {
            var session = await _context.GameSessions.FindAsync(gameSessionId);
            if (session == null || session.Status != "Waiting")
                return false;

            session.Status = "InProgress";
            session.StartedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Pauses an in-progress session.
        /// </summary>
        public async Task<bool> PauseSessionAsync(long gameSessionId)
        {
            var session = await _context.GameSessions.FindAsync(gameSessionId);
            if (session == null || session.Status != "InProgress")
                return false;

            session.Status = "Paused";
            session.PausedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Resumes a paused session.
        /// </summary>
        public async Task<bool> ResumeSessionAsync(long gameSessionId)
        {
            var session = await _context.GameSessions.FindAsync(gameSessionId);
            if (session == null || session.Status != "Paused")
                return false;

            session.Status = "InProgress";
            session.ResumedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Ends a game session, marking it as Completed.
        /// </summary>
        public async Task<bool> EndSessionAsync(long gameSessionId)
        {
            var session = await _context.GameSessions.FindAsync(gameSessionId);
            if (session == null)
                return false;

            session.Status = "Completed";
            session.EndedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Gets a session by its join code.
        /// </summary>
        public async Task<GameSession?> GetSessionByJoinCodeAsync(string joinCode)
        {
            return await _context.GameSessions
                .Include(s => s.Game)
                .Include(s => s.HostUser)
                .FirstOrDefaultAsync(s => s.JoinCode == joinCode && s.IsActive);
        }

        /// <summary>
        /// Gets a session by its primary key.
        /// </summary>
        public async Task<GameSession?> GetSessionByIdAsync(long gameSessionId)
        {
            return await _context.GameSessions
                .Include(s => s.Game)
                .FirstOrDefaultAsync(s => s.GameSessionId == gameSessionId);
        }

        #endregion

        #region Round Management

        /// <summary>
        /// Opens a new round in the session and assigns cards to all connected players.
        /// </summary>
        /// <param name="gameSessionId">The session to create the round in</param>
        /// <param name="connectedUserIds">List of UserIds currently connected to the session</param>
        /// <returns>The created GameRound</returns>
        public async Task<GameRound> OpenRoundAsync(long gameSessionId, List<long> connectedUserIds)
        {
            var session = await _context.GameSessions.FindAsync(gameSessionId);
            if (session == null)
                throw new InvalidOperationException("Session not found");

            byte roundNumber = session.CurrentRoundNumber;
            if (roundNumber < 1 || roundNumber > ROUND_TYPES.Length)
                throw new InvalidOperationException($"Invalid round number: {roundNumber}");

            string roundType = ROUND_TYPES[roundNumber - 1];

            var round = new GameRound
            {
                GameSessionId = gameSessionId,
                RoundNumber = roundNumber,
                RoundType = roundType,
                Status = "Open",
                OpenedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.GameRounds.Add(round);
            await _context.SaveChangesAsync();

            // Assign cards to all connected players
            await AssignCardsToPlayersAsync(round.GameRoundId, gameSessionId, roundType, connectedUserIds);

            return round;
        }

        /// <summary>
        /// Closes an open round.
        /// </summary>
        public async Task<bool> CloseRoundAsync(long gameRoundId)
        {
            var round = await _context.GameRounds.FindAsync(gameRoundId);
            if (round == null || round.Status != "Open")
                return false;

            round.Status = "Closed";
            round.ClosedAt = DateTime.UtcNow;

            var session = await _context.GameSessions.FindAsync(round.GameSessionId);
            if (session != null)
                session.CurrentRoundNumber++;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Assigns a card to a player who joined after the round was already opened.
        /// </summary>
        public async Task AssignLatePlayerAsync(long userId, GameRound round, long gameSessionId)
        {
            var session = await _context.GameSessions
                .Include(s => s.Game)
                .FirstOrDefaultAsync(s => s.GameSessionId == gameSessionId);

            if (session == null) return;

            var availableCards = await _context.Cards
                .Where(c => c.RoundType == round.RoundType
                         && c.CardType == "RoundCard"
                         && c.IsActive)
                .ToListAsync();

            if (!availableCards.Any()) return;

            var assignedCard = availableCards[_random.Next(availableCards.Count)];

            var ugc = new Ugc
            {
                UserId = userId,
                CardId = assignedCard.CardId,
                GameRoundId = round.GameRoundId,
                GameSessionId = gameSessionId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Ugcs.Add(ugc);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Returns the currently open round for a session, or null if no round is open.
        /// </summary>
        public async Task<GameRound?> GetOpenRoundAsync(long gameSessionId)
        {
            return await _context.GameRounds
                .FirstOrDefaultAsync(r => r.GameSessionId == gameSessionId && r.Status == "Open");
        }

        /// <summary>
        /// Returns true if every player assigned to a round has submitted their choice.
        /// </summary>
        public async Task<bool> AreAllPlayersSubmittedAsync(long gameRoundId)
        {
            var totalAssigned = await _context.Ugcs
                .CountAsync(u => u.GameRoundId == gameRoundId && u.Card.CardType == "RoundCard");
            var totalSubmitted = await _context.Ugcs
                .CountAsync(u => u.GameRoundId == gameRoundId && u.SubmittedAt != null && u.Card.CardType == "RoundCard");

            return totalAssigned > 0 && totalSubmitted >= totalAssigned;
        }

        /// <summary>
        /// Gets all non-responders for a round (players who didn't submit).
        /// </summary>
        public async Task<List<Ugc>> GetNonRespondersAsync(long gameRoundId)
        {
            return await _context.Ugcs
                .Include(u => u.User)
                .Include(u => u.Card)
                .Where(u => u.GameRoundId == gameRoundId && u.SubmittedAt == null)
                .ToListAsync();
        }

        #endregion

        #region Card Assignment and Submission

        /// <summary>
        /// Assigns cards to players for a specific round.
        /// </summary>
        private async Task AssignCardsToPlayersAsync(long gameRoundId, long gameSessionId, string roundType, List<long> userIds)
        {
            var session = await _context.GameSessions
                .Include(s => s.Game)
                .FirstOrDefaultAsync(s => s.GameSessionId == gameSessionId);

            if (session == null)
                return;

            var availableCards = await _context.Cards
                .Where(c => c.RoundType == roundType
                         && c.CardType == "RoundCard"
                         && c.IsActive)
                .ToListAsync();

            if (!availableCards.Any())
                throw new InvalidOperationException($"No cards available for round type: {roundType}");

            foreach (var userId in userIds)
            {
                Card assignedCard;

                switch (session.Game.Mode)
                {
                    case "RandomAssigned":
                        assignedCard = availableCards[_random.Next(availableCards.Count)];
                        break;

                    case "ChooseFromFour":
                        // Placeholder — actual card set when player submits their choice
                        assignedCard = availableCards.First();
                        break;

                    default:
                        assignedCard = availableCards.First();
                        break;
                }

                var ugc = new Ugc
                {
                    UserId = userId,
                    CardId = assignedCard.CardId,
                    GameRoundId = gameRoundId,
                    GameSessionId = gameSessionId,
                    AssignedAt = DateTime.UtcNow,
                    SubmittedAmount = null,
                    SubmittedAt = null,
                    RunningTotal = null,
                    IsActive = true
                };

                _context.Ugcs.Add(ugc);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets 4 random cards for a player to choose from in ChooseFromFour mode.
        /// </summary>
        public async Task<List<Card>> GetChoiceCardsForRoundAsync(string roundType, int count = 4)
        {
            var cards = await _context.Cards
                .Where(c => c.RoundType == roundType
                         && c.CardType == "RoundCard"
                         && c.IsActive)
                .ToListAsync();

            return cards.OrderBy(x => Guid.NewGuid()).Take(count).ToList();
        }

        /// <summary>
        /// Gets the player's pre-assigned UGC record for the current round (RandomAssigned mode).
        /// </summary>
        public async Task<Ugc?> GetPlayerRoundUgcAsync(long userId, long gameRoundId)
        {
            return await _context.Ugcs
                .Include(u => u.Card)
                .FirstOrDefaultAsync(u => u.UserId == userId
                    && u.GameRoundId == gameRoundId
                    && u.Card.CardType == "RoundCard");
        }

        /// <summary>
        /// Processes a player's card choice.
        /// For ChooseFromFour, updates the UGC with the chosen card.
        /// SubmittedAmount should be positive for Career (income) and negative for all other rounds (expenses).
        /// </summary>
        public async Task<bool> SubmitPlayerChoiceAsync(long userId, long gameRoundId, int chosenCardId, decimal submittedAmount)
        {
            var ugc = await _context.Ugcs
                .FirstOrDefaultAsync(u => u.UserId == userId && u.GameRoundId == gameRoundId);

            if (ugc == null || ugc.SubmittedAt != null)
                return false; // Already submitted or doesn't exist

            ugc.CardId = chosenCardId;
            ugc.SubmittedAmount = submittedAmount;
            ugc.SubmittedAt = DateTime.UtcNow;
            ugc.RunningTotal = await CalculateRunningTotalAsync(userId, ugc.GameSessionId, submittedAmount);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Calculates the running total for a player in a session.
        /// Running total represents the player's net monthly position after all rounds.
        /// Career round adds income; all other rounds subtract expenses.
        /// </summary>
        private async Task<decimal> CalculateRunningTotalAsync(long userId, long gameSessionId, decimal currentRoundAmount)
        {
            var priorUgc = await _context.Ugcs
                .Where(u => u.UserId == userId
                         && u.GameSessionId == gameSessionId
                         && u.SubmittedAt != null
                         && u.RunningTotal != null)
                .OrderByDescending(u => u.SubmittedAt)
                .FirstOrDefaultAsync();

            decimal priorTotal = priorUgc?.RunningTotal ?? 0;
            return priorTotal + currentRoundAmount;
        }

        #endregion

        #region Game Changers

        /// <summary>
        /// Returns whether a game changer should be assigned this round, based on difficulty.
        /// </summary>
        public bool ShouldAssignGameChanger(string? difficulty)
        {
            int chance = difficulty switch
            {
                "Easy" => 30,
                "Medium" => 60,
                "Hard" => 90,
                _ => 50
            };
            return _random.Next(100) < chance;
        }

        /// <summary>
        /// Assigns a random game changer card to a player for the current round.
        /// </summary>
        public async Task<Ugc?> AssignGameChangerAsync(long userId, long gameRoundId, long gameSessionId, string roundType)
        {
            var gameChangerCard = await _context.Cards
                .Where(c => c.CardType == "GameChangerCard"
                         && c.RoundType == roundType
                         && c.IsActive)
                .OrderBy(x => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (gameChangerCard == null)
                return null;

            decimal impact = gameChangerCard.MonthlyAmount ?? 0;

            var ugc = new Ugc
            {
                UserId = userId,
                CardId = gameChangerCard.CardId,
                GameRoundId = gameRoundId,
                GameSessionId = gameSessionId,
                AssignedAt = DateTime.UtcNow,
                SubmittedAmount = impact,
                SubmittedAt = DateTime.UtcNow, // Auto-applied
                RunningTotal = await CalculateRunningTotalAsync(userId, gameSessionId, impact),
                IsActive = true
            };

            _context.Ugcs.Add(ugc);
            await _context.SaveChangesAsync();

            return ugc;
        }

        /// <summary>
        /// Gets the game changer UGC assigned to a player for a specific round, if any.
        /// </summary>
        public async Task<Ugc?> GetPlayerGameChangerAsync(long userId, long gameRoundId)
        {
            return await _context.Ugcs
                .Include(u => u.Card)
                .FirstOrDefaultAsync(u => u.UserId == userId
                    && u.GameRoundId == gameRoundId
                    && u.Card.CardType == "GameChangerCard");
        }

        #endregion

        #region Financial State

        /// <summary>
        /// Computes the current financial state for a player based on their UGC history.
        /// Career round = monthly income. All other rounds = monthly expenses.
        /// Game changers affect the available balance but not recurring income/expenses.
        /// </summary>
        public async Task<PlayerFinancialState> GetPlayerFinancialStateAsync(long userId, long gameSessionId)
        {
            var ugcs = await _context.Ugcs
                .Include(u => u.Card)
                .Include(u => u.GameRound)
                .Where(u => u.UserId == userId
                    && u.GameSessionId == gameSessionId
                    && u.SubmittedAt != null)
                .ToListAsync();

            decimal monthlyIncome = 0;
            decimal monthlyExpenses = 0;
            decimal gameChangerBalance = 0;
            byte currentRound = 0;

            foreach (var ugc in ugcs)
            {
                var amount = ugc.SubmittedAmount ?? 0;
                var roundType = ugc.GameRound?.RoundType;

                if (ugc.Card?.CardType == "GameChangerCard")
                {
                    // Game changers are one-time balance adjustments
                    gameChangerBalance += amount;
                    continue;
                }

                if (roundType == "Career")
                    monthlyIncome = amount; // Career sets monthly income (positive)
                else
                    monthlyExpenses += Math.Abs(amount); // All other rounds are monthly expenses

                if (ugc.GameRound != null && ugc.GameRound.RoundNumber > currentRound)
                    currentRound = ugc.GameRound.RoundNumber;
            }

            decimal available = monthlyIncome - monthlyExpenses;
            decimal percentageAvailable = monthlyIncome > 0 ? (available / monthlyIncome) * 100 : 0;

            string financialHealth = percentageAvailable >= 30 ? "Healthy"
                : percentageAvailable >= 10 ? "Struggling"
                : "Critical";

            return new PlayerFinancialState
            {
                Salary = monthlyIncome * 12,
                MonthlyIncome = monthlyIncome,
                MonthlyExpenses = monthlyExpenses,
                Available = available,
                GameChangerBalance = gameChangerBalance,
                FinancialHealth = financialHealth,
                CurrentRound = currentRound
            };
        }

        #endregion

        #region Leaderboard and Reporting

        /// <summary>
        /// Gets the current leaderboard for a session.
        /// Returns players ordered by their most recent RunningTotal.
        /// </summary>
        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(long gameSessionId)
        {
            var leaderboard = await _context.Ugcs
                .Where(u => u.GameSessionId == gameSessionId && u.SubmittedAt != null)
                .GroupBy(u => u.UserId)
                .Select(g => new LeaderboardEntry
                {
                    UserId = g.Key,
                    Username = g.First().User.Username,
                    CurrentScore = g.Max(u => u.RunningTotal) ?? 0,
                    CardsSubmitted = g.Count()
                })
                .OrderByDescending(l => l.CurrentScore)
                .ToListAsync();

            return leaderboard;
        }

        /// <summary>
        /// Gets all card interactions for a specific user in a session.
        /// </summary>
        public async Task<List<Ugc>> GetPlayerHistoryAsync(long userId, long gameSessionId)
        {
            return await _context.Ugcs
                .Include(u => u.Card)
                .Include(u => u.GameRound)
                .Where(u => u.UserId == userId && u.GameSessionId == gameSessionId)
                .OrderBy(u => u.AssignedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all results for a specific round with card details.
        /// </summary>
        public async Task<List<RoundResult>> GetRoundResultsAsync(long gameRoundId)
        {
            return await _context.Ugcs
                .Include(u => u.User)
                .Include(u => u.Card)
                .Where(u => u.GameRoundId == gameRoundId)
                .Select(u => new RoundResult
                {
                    Username = u.User.Username,
                    CardTitle = u.Card.Title,
                    CorrectAmount = u.Card.MonthlyAmount ?? 0,
                    SubmittedAmount = u.SubmittedAmount,
                    RunningTotal = u.RunningTotal,
                    IsSubmitted = u.SubmittedAt != null
                })
                .OrderByDescending(r => r.RunningTotal)
                .ToListAsync();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generates a random 5-digit join code.
        /// </summary>
        private string GenerateJoinCode()
        {
            return _random.Next(10000, 99999).ToString();
        }

        #endregion
    }

    #region DTOs

    public class LeaderboardEntry
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public decimal CurrentScore { get; set; }
        public int CardsSubmitted { get; set; }
    }

    public class RoundResult
    {
        public string Username { get; set; } = string.Empty;
        public string CardTitle { get; set; } = string.Empty;
        public decimal CorrectAmount { get; set; }
        public decimal? SubmittedAmount { get; set; }
        public decimal? RunningTotal { get; set; }
        public bool IsSubmitted { get; set; }
    }

    public class PlayerFinancialState
    {
        public decimal Salary { get; set; }           // Annual estimate (MonthlyIncome * 12)
        public decimal MonthlyIncome { get; set; }    // From Career round
        public decimal MonthlyExpenses { get; set; }  // Sum of all non-Career round costs
        public decimal Available { get; set; }         // MonthlyIncome - MonthlyExpenses
        public decimal GameChangerBalance { get; set; } // One-time game changer adjustments
        public string FinancialHealth { get; set; } = "Healthy"; // Healthy / Struggling / Critical
        public byte CurrentRound { get; set; }        // Most recently completed round number
    }

    #endregion
}
