using DealtHands.ModelsV2;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class GameChangerModel : PageModel
    {
        private readonly GameSessionService _gameSessionService;

        public GameChangerModel(GameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        // The game changer UGC record (contains the Card with title, description, and financial impact)
        public Ugc GameChangerUgc { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Hard stop — educators never receive game changers
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

            // Find the open round to look up the game changer assigned to this player
            var openRound = await _gameSessionService.GetOpenRoundAsync(gameSessionId);
            if (openRound != null)
                GameChangerUgc = await _gameSessionService.GetPlayerGameChangerAsync(userId, openRound.GameRoundId);

            return Page();
        }

        // Player clicks Continue after viewing their game changer card
        public IActionResult OnPostContinue()
        {
            return RedirectToPage("/Round");
        }
    }
}