using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class CreateSessionModel : PageModel
    {
        private readonly GameSessionService _gameSessionService;
        private readonly IAuthenticationService _authService;

        public CreateSessionModel(GameSessionService gameSessionService, IAuthenticationService authService)
        {
            _gameSessionService = gameSessionService;
            _authService = authService;
        }

        [BindProperty]
        public string SessionName { get; set; }

        [BindProperty]
        public string GameMode { get; set; }

        [BindProperty]
        public string Difficulty { get; set; }

        [BindProperty]
        public int MaxPlayers { get; set; } = 35;

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is authenticated as educator
            if (!_authService.IsEducator)
                return RedirectToPage("/Login");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate inputs before processing
            if (string.IsNullOrWhiteSpace(SessionName) || SessionName.Length > 100)
            {
                ModelState.AddModelError(nameof(SessionName), "Session name is required and must be 100 characters or less.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(GameMode) || (GameMode != "RandomAssigned" && GameMode != "ChooseFromFour"))
            {
                ModelState.AddModelError(nameof(GameMode), "Invalid game mode selected.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Difficulty) || (Difficulty != "Easy" && Difficulty != "Medium" && Difficulty != "Hard"))
            {
                ModelState.AddModelError(nameof(Difficulty), "Invalid difficulty selected.");
                return Page();
            }

            if (MaxPlayers < 1 || MaxPlayers > 100)
            {
                ModelState.AddModelError(nameof(MaxPlayers), "Max players must be between 1 and 100.");
                return Page();
            }

            if (!ModelState.IsValid) return Page();

            // Check if user is authenticated as educator
            if (!_authService.IsEducator || !_authService.UserId.HasValue)
                return RedirectToPage("/Login");

            // Map game mode selection to the corresponding Game row
            // GameId 1 = RandomAssigned, GameId 2 = ChooseFromFour
            long gameId = GameMode == "ChooseFromFour" ? 2 : 1;

            var session = await _gameSessionService.CreateSessionAsync(
                _authService.UserId.Value,
                gameId,
                SessionName,
                Difficulty);

            // Store session code in the authentication service
            _authService.SetSessionCode(session.JoinCode);

            return RedirectToPage("/Lobby", new { sessionCode = session.JoinCode });
        }
    }
}
