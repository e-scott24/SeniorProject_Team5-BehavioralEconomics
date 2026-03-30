using DealtHands.ModelsV2;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class RoundModel : PageModel
    {
        private readonly GameSessionService _gameSessionService;
        private readonly SessionTracker _sessionTracker;
        private readonly IAuthenticationService _authService;

        public RoundModel(GameSessionService gameSessionService, SessionTracker sessionTracker,
                          IAuthenticationService authService)
        {
            _gameSessionService = gameSessionService;
            _sessionTracker = sessionTracker;
            _authService = authService;
        }

        public List<Card> Cards { get; set; } = new List<Card>();
        public Ugc AssignedCard { get; set; }
        public GameRound CurrentRound { get; set; }
        public GameSession Session { get; set; }
        public PlayerFinancialState FinancialState { get; set; }
        public string WaitingMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Hard stop — educators never play rounds
            if (_authService.IsEducator)
            {
                var code = _authService.SessionCode;
                return !string.IsNullOrEmpty(code)
                    ? RedirectToPage("/Lobby", new { sessionCode = code })
                    : RedirectToPage("/EducatorDashboard");
            }

            // Must be a student with valid session data
            if (!_authService.UserId.HasValue)
                return RedirectToPage("/JoinSession");

            if (!_authService.GameSessionId.HasValue)
                return RedirectToPage("/JoinSession");

            long userId = _authService.UserId.Value;
            long gameSessionId = _authService.GameSessionId.Value;

            Session = await _gameSessionService.GetSessionByIdAsync(gameSessionId);
            if (Session == null) return RedirectToPage("/JoinSession");

            if (Session.Status == "Completed")
                return RedirectToPage("/Results");

            if (Session.Status == "Paused")
                return RedirectToPage("/JoinSession");

            CurrentRound = await _gameSessionService.GetOpenRoundAsync(gameSessionId);
            if (CurrentRound == null)
            {
                WaitingMessage = "Waiting for the educator to open the next round...";
                FinancialState = await _gameSessionService.GetPlayerFinancialStateAsync(userId, gameSessionId);
                return Page();
            }

            // Check if this player already submitted for this round
            var existingUgc = await _gameSessionService.GetPlayerRoundUgcAsync(userId, CurrentRound.GameRoundId);
            if (existingUgc?.SubmittedAt != null)
            {
                WaitingMessage = "Choice submitted! Waiting for the educator to advance to the next round...";
                FinancialState = await _gameSessionService.GetPlayerFinancialStateAsync(userId, gameSessionId);
                return Page();
            }

            // Player wasn't in the tracker when the round opened — assign them a card now
            if (existingUgc == null)
            {
                _sessionTracker.AddPlayer(gameSessionId, userId);
                await _gameSessionService.AssignLatePlayerAsync(userId, CurrentRound, gameSessionId);
                existingUgc = await _gameSessionService.GetPlayerRoundUgcAsync(userId, CurrentRound.GameRoundId);
            }

            if (Session.Game?.Mode == "RandomAssigned")
                AssignedCard = existingUgc;
            else
                Cards = await _gameSessionService.GetChoiceCardsForRoundAsync(CurrentRound.RoundType);

            FinancialState = await _gameSessionService.GetPlayerFinancialStateAsync(userId, gameSessionId);
            WaitingMessage = TempData["WaitingMessage"] as string;

            return Page();
        }

        public async Task<IActionResult> OnPostSelectChoiceAsync(int chosenCardId, decimal submittedAmount)
        {
            // Hard stop — educators never post choices
            if (_authService.IsEducator)
            {
                var code = _authService.SessionCode;
                return !string.IsNullOrEmpty(code)
                    ? RedirectToPage("/Lobby", new { sessionCode = code })
                    : RedirectToPage("/EducatorDashboard");
            }

            if (!_authService.UserId.HasValue)
                return RedirectToPage("/JoinSession");

            if (!_authService.GameSessionId.HasValue)
                return RedirectToPage("/JoinSession");

            long userId = _authService.UserId.Value;
            long gameSessionId = _authService.GameSessionId.Value;

            Session = await _gameSessionService.GetSessionByIdAsync(gameSessionId);
            if (Session == null) return RedirectToPage("/JoinSession");

            CurrentRound = await _gameSessionService.GetOpenRoundAsync(gameSessionId);
            if (CurrentRound == null) return RedirectToPage("/Round");

            // Career = income (positive), all other rounds = expenses (negative)
            decimal adjustedAmount = CurrentRound.RoundType == "Career"
                ? submittedAmount
                : -submittedAmount;

            bool success = await _gameSessionService.SubmitPlayerChoiceAsync(
                userId, CurrentRound.GameRoundId, chosenCardId, adjustedAmount);

            if (!success)
            {
                TempData["WaitingMessage"] = "Submission failed or already submitted.";
                return RedirectToPage("/Round");
            }

            // Check if a game changer should be assigned
            if (_gameSessionService.ShouldAssignGameChanger(Session.Difficulty))
            {
                var gc = await _gameSessionService.AssignGameChangerAsync(
                    userId, CurrentRound.GameRoundId, gameSessionId, CurrentRound.RoundType);

                if (gc != null)
                    return RedirectToPage("/GameChanger");
            }

            TempData["WaitingMessage"] = "Choice submitted! Waiting for the educator to advance to the next round...";
            return RedirectToPage("/Round");
        }

        /// <summary>
        /// API endpoint for students to poll and detect when:
        /// - Current round has closed
        /// - New round has opened
        /// - Game has completed
        /// </summary>
        public async Task<JsonResult> OnGetCheckRoundStatusAsync(long gameSessionId)
        {
            // Must be a student
            if (_authService.IsEducator)
                return new JsonResult(new { error = "Educators don't poll rounds" });

            if (!_authService.UserId.HasValue)
                return new JsonResult(new { error = "Not authenticated" });

            var session = await _gameSessionService.GetSessionByIdAsync(gameSessionId);
            if (session == null)
                return new JsonResult(new { error = "Session not found" });

            // Check if game completed
            if (session.Status == "Completed")
            {
                return new JsonResult(new
                {
                    gameCompleted = true,
                    currentRoundClosed = false,
                    newRoundOpen = false,
                    playerSubmitted = false
                });
            }

            // Get the current open round
            var openRound = await _gameSessionService.GetOpenRoundAsync(gameSessionId);

            // Check if player has submitted for current round
            bool playerSubmitted = false;
            if (openRound != null)
            {
                var ugc = await _gameSessionService.GetPlayerRoundUgcAsync(_authService.UserId.Value, openRound.GameRoundId);
                playerSubmitted = ugc?.SubmittedAt != null;
            }

            return new JsonResult(new
            {
                gameCompleted = false,
                currentRoundClosed = openRound == null, // No open round = current round closed
                newRoundOpen = openRound != null, // Open round exists = new round available
                playerSubmitted = playerSubmitted
            });
        }
    }
}