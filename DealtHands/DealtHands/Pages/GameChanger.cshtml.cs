using DealtHands.ModelsV2;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class GameChangerModel : PageModel
    {
        private readonly GameSessionService _gameSessionService;
        private readonly IAuthenticationService _authService;

        public GameChangerModel(GameSessionService gameSessionService, IAuthenticationService authService)
        {
            _gameSessionService = gameSessionService;
            _authService = authService;
        }

        public Ugc GameChangerUgc { get; set; }
        public PlayerFinancialState? FinancialState { get; set; }

        public async Task<IActionResult>
    OnGetAsync()
        {
            // Hard stop — educators never receive game changers
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

            var openRound = await _gameSessionService.GetOpenRoundAsync(gameSessionId);
            if (openRound != null)
                GameChangerUgc = await _gameSessionService.GetPlayerGameChangerAsync(userId, openRound.GameRoundId);

            FinancialState = await _gameSessionService.GetPlayerFinancialStateAsync(userId, gameSessionId);

            return Page();
        }

        // Player clicks Continue after viewing their game changer card
        public IActionResult OnPostContinue()
        {
            return RedirectToPage("/Round");
        }
    }
}
