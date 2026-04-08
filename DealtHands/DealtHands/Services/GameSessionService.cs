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
        /// <param name="gameId">The GameId corresponding to the selected mode (1 = RandomAssigned, 2 = ChooseFromFour)</param>
        /// <param name="sessionName">Optional friendly name for the session</param>
        /// <param name="difficulty">"Easy", "Medium", or "Hard" — affects game changer probability</param>
        /// <returns>The created GameSession with a unique JoinCode</returns>
        public async Task<GameSession> CreateSessionAsync(long hostUserId, long gameId, string? sessionName = null, string? difficulty = null)
        {
            string joinCode;
            do
            {
                joinCode = GenerateJoinCode();
            }
            while (await _context.GameSessions.AnyAsync(s => s.JoinCode == joinCode && s.IsActive));

            var session = new GameSession
            {
                GameId = gameId,
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
            bool alreadyAssigned = await _context.Ugcs
                .AnyAsync(u => u.UserId == userId && u.GameRoundId == round.GameRoundId);
            if (alreadyAssigned) return;

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
                GameChangerId = null,
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
                .CountAsync(u => u.GameRoundId == gameRoundId && u.CardId != null);
            var totalSubmitted = await _context.Ugcs
                .CountAsync(u => u.GameRoundId == gameRoundId && u.CardId != null && u.SubmittedAt != null);

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
                .Where(u => u.GameRoundId == gameRoundId && u.CardId != null && u.SubmittedAt == null)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all rounds for a specific session, ordered by round number.
        /// </summary>
        public async Task<List<GameRound>> GetSessionRoundsAsync(long gameSessionId)
        {
            return await _context.GameRounds
                .Where(r => r.GameSessionId == gameSessionId)
                .OrderBy(r => r.RoundNumber)
                .ToListAsync();
        }

        #endregion

        #region Card Assignment and Submission

        /// <summary>
        /// Assigns round cards to players for a specific round.
        /// Skips any player who already has a UGC record for this round to prevent duplicate key errors.
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
                // Guard against duplicate assignment if this method is called more than once for the same round
                bool alreadyAssigned = await _context.Ugcs
                    .AnyAsync(u => u.UserId == userId && u.GameRoundId == gameRoundId);
                if (alreadyAssigned) continue;

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
                    GameChangerId = null,
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
                    && u.CardId != null);
        }

        public async Task<bool> SubmitPlayerChoiceAsync(long userId, long gameRoundId, int chosenCardId, decimal submittedAmount)
        {
            var ugc = await _context.Ugcs
                .FirstOrDefaultAsync(u => u.UserId == userId
                    && u.GameRoundId == gameRoundId
                    && u.CardId != null);

            if (ugc == null || ugc.SubmittedAt != null)
                return false;

            ugc.CardId = chosenCardId;
            ugc.SubmittedAmount = submittedAmount;
            ugc.SubmittedAt = DateTime.UtcNow;
            ugc.RunningTotal = await CalculateRunningTotalAsync(userId, ugc.GameSessionId, submittedAmount);

            var card = await _context.Cards.FindAsync(chosenCardId);
            if (card != null && card.CardType == "RoundCard")
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    if (card.SetsStudentLoans.HasValue) user.HasStudentLoans = card.SetsStudentLoans.Value;
                    if (card.SetsCar.HasValue) user.HasCar = card.SetsCar.Value;
                    if (card.SetsCarLoan.HasValue) user.HasCarLoan = card.SetsCarLoan.Value;
                    if (card.SetsOwnsHome.HasValue) user.OwnsHome = card.SetsOwnsHome.Value;
                    if (card.SetsApartment.HasValue) user.HasApartment = card.SetsApartment.Value;
                    if (card.SetsRoommate.HasValue) user.HasRoommate = card.SetsRoommate.Value;
                    if (card.SetsMarried.HasValue) user.IsMarried = card.SetsMarried.Value;
                    if (card.SetsChildren.HasValue) user.HasChildren = card.SetsChildren.Value;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

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
        /// Assigns a random game changer to a player for the current round.
        /// Filters by round type, session difficulty, and player state flags.
        /// If no candidates match the player's flags, no game changer is assigned.
        /// Applies Sets* flags to the player's User record immediately on assignment.
        /// </summary>
        public async Task<Ugc?> AssignGameChangerAsync(long userId, long gameRoundId, long gameSessionId, string roundType)
        {
            // Guard against duplicate game changer assignment for the same player/round
            bool alreadyAssigned = await _context.Ugcs
                .AnyAsync(u => u.UserId == userId && u.GameRoundId == gameRoundId && u.GameChangerId != null);
            if (alreadyAssigned) return null;

            var user = await _context.Users.FindAsync(userId);
            var session = await _context.GameSessions.FindAsync(gameSessionId);

            // Convert difficulty string to numeric level for filtering
            byte sessionDifficulty = (session?.Difficulty) switch
            {
                "Easy" => 1,
                "Medium" => 2,
                "Hard" => 3,
                _ => 2
            };

            // Load candidates from GameChanger table filtered by round type and difficulty
            var candidates = await _context.GameChangers
                .Where(g => g.RoundType == roundType
                         && g.IsActive
                         && g.DifficultyLevel <= sessionDifficulty)
                .ToListAsync();

            // Filter in memory by player state flags
            // NULL = don't care, true = must have, false = must not have
            if (user != null)
            {
                candidates = candidates.Where(g =>
                    (!g.RequiresStudentLoans.HasValue || g.RequiresStudentLoans.Value == user.HasStudentLoans) &&
                    (!g.RequiresCar.HasValue || g.RequiresCar.Value == user.HasCar) &&
                    (!g.RequiresCarLoan.HasValue || g.RequiresCarLoan.Value == user.HasCarLoan) &&
                    (!g.RequiresOwnsHome.HasValue || g.RequiresOwnsHome.Value == user.OwnsHome) &&
                    (!g.RequiresApartment.HasValue || g.RequiresApartment.Value == user.HasApartment) &&
                    (!g.RequiresRoommate.HasValue || g.RequiresRoommate.Value == user.HasRoommate) &&
                    (!g.RequiresMarried.HasValue || g.RequiresMarried.Value == user.IsMarried) &&
                    (!g.RequiresChildren.HasValue || g.RequiresChildren.Value == user.HasChildren) &&
                    (!g.RequiresJob.HasValue || g.RequiresJob.Value == user.HasJob)
                ).ToList();
            }

            // If no candidates match the player's flags, do not assign a game changer.
            // Falling back to flag-free filtering would assign cards that make no sense
            // for this player's situation (e.g. a loan card to a player with no loans).
            if (!candidates.Any())
                return null;

            var gameChanger = candidates[_random.Next(candidates.Count)];

            // Only MonthlyAmount is stored as SubmittedAmount on the UGC row (one-time balance hit).
            // IncomeEffect, IncomeEffectPercent, and ExpenseEffect are read directly from the
            // GameChanger record each time financial state is calculated — they are not stored on UGC.
            decimal oneTimeImpact = gameChanger.MonthlyAmount ?? 0;

            // Apply Sets* flags to the player's state immediately
            if (user != null)
            {
                if (gameChanger.SetsStudentLoans.HasValue) user.HasStudentLoans = gameChanger.SetsStudentLoans.Value;
                if (gameChanger.SetsCar.HasValue) user.HasCar = gameChanger.SetsCar.Value;
                if (gameChanger.SetsCarLoan.HasValue) user.HasCarLoan = gameChanger.SetsCarLoan.Value;
                if (gameChanger.SetsOwnsHome.HasValue) user.OwnsHome = gameChanger.SetsOwnsHome.Value;
                if (gameChanger.SetsApartment.HasValue) user.HasApartment = gameChanger.SetsApartment.Value;
                if (gameChanger.SetsRoommate.HasValue) user.HasRoommate = gameChanger.SetsRoommate.Value;
                if (gameChanger.SetsMarried.HasValue) user.IsMarried = gameChanger.SetsMarried.Value;
                if (gameChanger.SetsChildren.HasValue) user.HasChildren = gameChanger.SetsChildren.Value;
                if (gameChanger.SetsJob.HasValue) user.HasJob = gameChanger.SetsJob.Value;
            }

            var ugc = new Ugc
            {
                UserId = userId,
                CardId = null,
                GameChangerId = gameChanger.GameChangerId,
                GameRoundId = gameRoundId,
                GameSessionId = gameSessionId,
                AssignedAt = DateTime.UtcNow,
                SubmittedAmount = oneTimeImpact,
                SubmittedAt = DateTime.UtcNow, // Auto-applied
                RunningTotal = await CalculateRunningTotalAsync(userId, gameSessionId, oneTimeImpact),
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
                .Include(u => u.GameChanger)
                .FirstOrDefaultAsync(u => u.UserId == userId
                    && u.GameRoundId == gameRoundId
                    && u.GameChangerId != null);
        }

        /// <summary>
        /// Gets a GameChanger by its primary key. Used by the Round page overlay.
        /// </summary>
        public async Task<GameChanger?> GetGameChangerByIdAsync(int gameChangerId)
        {
            return await _context.GameChangers.FindAsync(gameChangerId);
        }

        #endregion

        #region Financial State

        /// <summary>
        /// Computes the current financial state for a player based on their UGC history.
        ///
        /// Two-pass calculation:
        ///   Pass 1: Establish base monthly income (Career round) and base expenses (all other rounds).
        ///           BaseMonthlyIncome and BaseMonthlyExpenses are captured here before any GC effects.
        ///   Pass 2: Apply game changer effects — IncomeEffectPercent and IncomeEffect modify income,
        ///           ExpenseEffect modifies expenses, MonthlyAmount is a one-time balance adjustment.
        ///
        /// BaseMonthlyIncome is exposed on the DTO so the GC overlay can compute real dollar
        /// amounts for percentage-based cards (e.g. Laid Off = -100% of base income).
        /// </summary>
        public async Task<PlayerFinancialState> GetPlayerFinancialStateAsync(long userId, long gameSessionId)
        {
            var ugcs = await _context.Ugcs
                .Include(u => u.Card)
                .Include(u => u.GameChanger)
                .Include(u => u.GameRound)
                .Where(u => u.UserId == userId
                    && u.GameSessionId == gameSessionId
                    && u.SubmittedAt != null)
                .ToListAsync();

            decimal monthlyIncome = 0;
            decimal monthlyExpenses = 0;
            decimal gameChangerBalance = 0;
            byte currentRound = 0;

            // Pass 1: Establish base income and expenses from round card submissions
            foreach (var ugc in ugcs.Where(u => u.GameChangerId == null))
            {
                var amount = ugc.SubmittedAmount ?? 0;
                var roundType = ugc.GameRound?.RoundType;

                if (roundType == "Career")
                    monthlyIncome = amount;
                else
                    monthlyExpenses += Math.Abs(amount);

                if (ugc.GameRound != null && ugc.GameRound.RoundNumber > currentRound)
                    currentRound = ugc.GameRound.RoundNumber;
            }

            // Capture base values before GC effects for percentage display in the overlay
            decimal baseMonthlyIncome = monthlyIncome;
            decimal baseMonthlyExpenses = monthlyExpenses;

            // Pass 2: Apply game changer effects on top of base income/expenses
            foreach (var ugc in ugcs.Where(u => u.GameChangerId != null))
            {
                var gc = ugc.GameChanger;
                if (gc == null) continue;

                // One-time balance hit (e.g. DUI fine, medical bill, bonus payout)
                gameChangerBalance += ugc.SubmittedAmount ?? 0;

                // Percentage-based income modifier (e.g. Laid Off = -1.0, Promoted = +0.15)
                if (gc.IncomeEffectPercent.HasValue)
                    monthlyIncome = Math.Max(0, monthlyIncome * (1m + gc.IncomeEffectPercent.Value));

                // Flat income modifier (e.g. Wage Garnishment = -350, Spouse Got a Raise = +550)
                if (gc.IncomeEffect.HasValue)
                    monthlyIncome = Math.Max(0, monthlyIncome + gc.IncomeEffect.Value);

                // Flat expense modifier (e.g. Rent Increase = +200, Roommate Found = -450)
                if (gc.ExpenseEffect.HasValue)
                    monthlyExpenses = Math.Max(0, monthlyExpenses + gc.ExpenseEffect.Value);
            }

            decimal available = monthlyIncome - monthlyExpenses + gameChangerBalance;
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
                CurrentRound = currentRound,
                BaseMonthlyIncome = baseMonthlyIncome,
                BaseMonthlyExpenses = baseMonthlyExpenses
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
            var allUgcs = await _context.Ugcs
                .Include(u => u.User)
                .Where(u => u.GameSessionId == gameSessionId && u.SubmittedAt != null)
                .OrderByDescending(u => u.SubmittedAt)
                .ToListAsync();

            var leaderboard = allUgcs
                .GroupBy(u => u.UserId)
                .Select(g => new LeaderboardEntry
                {
                    UserId = g.Key,
                    Username = g.First().User.Username,
                    CurrentScore = g.First().RunningTotal ?? 0,
                    CardsSubmitted = g.Count()
                })
                .OrderByDescending(l => l.CurrentScore)
                .ToList();

            return leaderboard;
        }

        /// <summary>
        /// Gets all card interactions for a specific user in a session.
        /// </summary>
        public async Task<List<Ugc>> GetPlayerHistoryAsync(long userId, long gameSessionId)
        {
            return await _context.Ugcs
                .Include(u => u.Card)
                .Include(u => u.GameChanger)
                .Include(u => u.GameRound)
                .Where(u => u.UserId == userId && u.GameSessionId == gameSessionId)
                .OrderBy(u => u.AssignedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all results for a specific round with card details.
        /// Includes both round card submissions and game changer assignments.
        /// </summary>
        public async Task<List<RoundResult>> GetRoundResultsAsync(long gameRoundId)
        {
            var ugcs = await _context.Ugcs
                .Include(u => u.User)
                .Include(u => u.Card)
                .Include(u => u.GameChanger)
                .Where(u => u.GameRoundId == gameRoundId)
                .ToListAsync();

            return ugcs.Select(u => new RoundResult
            {
                Username = u.User.Username,
                CardTitle = u.Card?.Title ?? u.GameChanger?.Title ?? string.Empty,
                CorrectAmount = u.Card?.MonthlyAmount ?? u.GameChanger?.MonthlyAmount ?? 0,
                SubmittedAmount = u.SubmittedAmount,
                RunningTotal = u.RunningTotal,
                IsSubmitted = u.SubmittedAt != null
            })
            .OrderByDescending(r => r.RunningTotal)
            .ToList();
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
        public decimal Salary { get; set; }              // Annual estimate (MonthlyIncome * 12)
        public decimal MonthlyIncome { get; set; }       // From Career round, modified by game changers
        public decimal MonthlyExpenses { get; set; }     // Sum of all non-Career round costs, modified by game changers
        public decimal Available { get; set; }            // MonthlyIncome - MonthlyExpenses + GameChangerBalance
        public decimal GameChangerBalance { get; set; }  // One-time game changer balance adjustments
        public string FinancialHealth { get; set; } = "Healthy"; // Healthy / Struggling / Critical
        public byte CurrentRound { get; set; }           // Most recently completed round number
        public decimal BaseMonthlyIncome { get; set; }   // Income before GC effects — used for percentage display
        public decimal BaseMonthlyExpenses { get; set; } // Expenses before GC effects — used for percentage display
    }

    #endregion
}