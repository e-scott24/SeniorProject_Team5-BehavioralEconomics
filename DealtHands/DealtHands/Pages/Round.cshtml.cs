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

        public RoundModel(GameSessionService gameSessionService, SessionTracker sessionTracker)
        {
            _gameSessionService = gameSessionService;
            _sessionTracker = sessionTracker;
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
            if (HttpContext.Session.GetString("Role") == "Educator")
            {
                var code = HttpContext.Session.GetString("SessionCode");
                return !string.IsNullOrEmpty(code)
                    ? RedirectToPage("/Lobby", new { sessionCode = code })
                    : RedirectToPage("/EducatorDashboard");
            }

            // Must be a student with valid session data
            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long userId))
                return RedirectToPage("/JoinSession");
            if (!long.TryParse(HttpContext.Session.GetString("GameSessionId"), out long gameSessionId))
                return RedirectToPage("/JoinSession");

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
            if (HttpContext.Session.GetString("Role") == "Educator")
            {
                var code = HttpContext.Session.GetString("SessionCode");
                return !string.IsNullOrEmpty(code)
                    ? RedirectToPage("/Lobby", new { sessionCode = code })
                    : RedirectToPage("/EducatorDashboard");
            }

            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long userId))
                return RedirectToPage("/JoinSession");
            if (!long.TryParse(HttpContext.Session.GetString("GameSessionId"), out long gameSessionId))
                return RedirectToPage("/JoinSession");

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
            if (HttpContext.Session.GetString("Role") == "Educator")
                return new JsonResult(new { error = "Educators don't poll rounds" });

            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long userId))
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
                var ugc = await _gameSessionService.GetPlayerRoundUgcAsync(userId, openRound.GameRoundId);
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
