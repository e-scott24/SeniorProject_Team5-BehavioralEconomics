using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class CreateSessionModel : PageModel
    {
        private readonly GameSessionService _gameSessionService;

        public CreateSessionModel(GameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        [BindProperty]
        public string SessionName { get; set; }

        [BindProperty]
        public string GameMode { get; set; }

        [BindProperty]
        public string Difficulty { get; set; }

        [BindProperty]
        public int MaxPlayers { get; set; } = 35;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long userId))
                return RedirectToPage("/Login");

            // Confirm educator role is set — should already be set from login
            // but explicitly re-set here to be safe
            HttpContext.Session.SetString("Role", "Educator");

            var session = await _gameSessionService.CreateSessionAsync(userId, SessionName, Difficulty);

            // Store session info for the educator's control panel
            HttpContext.Session.SetString("GameSessionId", session.GameSessionId.ToString());
            HttpContext.Session.SetString("SessionCode", session.JoinCode);

            return RedirectToPage("/Lobby", new { sessionCode = session.JoinCode });
        }
    }
}
