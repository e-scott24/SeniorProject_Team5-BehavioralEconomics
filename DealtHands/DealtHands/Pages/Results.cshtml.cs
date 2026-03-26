using DealtHands.ModelsV2;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class ResultsModel : PageModel
    {
        private readonly GameSessionService _gameSessionService;

        public ResultsModel(GameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        public List<LeaderboardEntry> Leaderboard { get; set; } = new List<LeaderboardEntry>();
        public List<Ugc> PlayerHistory { get; set; } = new List<Ugc>();
        public PlayerFinancialState FinancialState { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Hard stop — educators don't have a player GameSessionId
            if (HttpContext.Session.GetString("Role") == "Educator")
                return RedirectToPage("/EducatorDashboard");

            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long userId))
                return RedirectToPage("/JoinSession");
            if (!long.TryParse(HttpContext.Session.GetString("GameSessionId"), out long gameSessionId))
                return RedirectToPage("/JoinSession");

            Leaderboard = await _gameSessionService.GetLeaderboardAsync(gameSessionId);
            PlayerHistory = await _gameSessionService.GetPlayerHistoryAsync(userId, gameSessionId);
            FinancialState = await _gameSessionService.GetPlayerFinancialStateAsync(userId, gameSessionId);

            return Page();
        }
    }
}