using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class JoinSessionModel : PageModel
    {
        private readonly SessionService _sessionService;
        private readonly PlayerService _playerService;

        public JoinSessionModel(SessionService sessionService, PlayerService playerService)
        {
            _sessionService = sessionService;
            _playerService = playerService;
        }

        [BindProperty]
        public string SessionCode { get; set; }

        [BindProperty]
        public string PlayerName { get; set; }

        [BindProperty]
        public string PlayerCode { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            // Validate session code
            if (string.IsNullOrEmpty(SessionCode))
            {
                ErrorMessage = "Please enter a session code.";
                return Page();
            }

            var session = _sessionService.GetSessionByCode(SessionCode);

            if (session == null)
            {
                ErrorMessage = "Invalid session code.";
                return Page();
            }

            // Returning player with code
            if (!string.IsNullOrEmpty(PlayerCode))
            {
                var player = _playerService.JoinSession(session.Id, null, PlayerCode);

                if (player == null)
                {
                    ErrorMessage = "Invalid player code for this session.";
                    return Page();
                }

                return RedirectToPage("/Lobby", new { sessionCode = SessionCode, playerId = player.Id });
            }

            // New player with name
            if (string.IsNullOrEmpty(PlayerName))
            {
                ErrorMessage = "Please enter your name.";
                return Page();
            }

            // Check if session is full
            var currentPlayers = _playerService.GetPlayersInSession(session.Id);
            if (currentPlayers.Count >= session.MaxPlayers)
            {
                ErrorMessage = "This session is full.";
                return Page();
            }

            var newPlayer = _playerService.JoinSession(session.Id, PlayerName);

            return RedirectToPage("/Lobby", new { sessionCode = SessionCode, playerId = newPlayer.Id });
        }
    }
}