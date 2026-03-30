using DealtHands.ModelsV2;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class ResultsModel : PageModel
    {
        private readonly GameSessionService _gameSessionService;
        private readonly IAuthenticationService _authService;

        public ResultsModel(GameSessionService gameSessionService, IAuthenticationService authService)
        {
            _gameSessionService = gameSessionService;
            _authService = authService;
        }

        public List<LeaderboardEntry> Leaderboard { get; set; } = new List<LeaderboardEntry>();
        public List<Ugc> PlayerHistory { get; set; } = new List<Ugc>();
        public PlayerFinancialState FinancialState { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Hard stop — educators don't have a player GameSessionId
            if (_authService.IsEducator)
                return RedirectToPage("/EducatorDashboard");

            if (!_authService.UserId.HasValue)
                return RedirectToPage("/JoinSession");

            if (!_authService.GameSessionId.HasValue)
                return RedirectToPage("/JoinSession");

            long userId = _authService.UserId.Value;
            long gameSessionId = _authService.GameSessionId.Value;

            Leaderboard = await _gameSessionService.GetLeaderboardAsync(gameSessionId);
            PlayerHistory = await _gameSessionService.GetPlayerHistoryAsync(userId, gameSessionId);
            FinancialState = await _gameSessionService.GetPlayerFinancialStateAsync(userId, gameSessionId);

            return Page();
        }
    }
}