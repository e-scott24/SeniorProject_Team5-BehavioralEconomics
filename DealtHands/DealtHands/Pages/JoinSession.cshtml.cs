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

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            // Page loads
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(SessionCode) || string.IsNullOrEmpty(PlayerName))
            {
                ErrorMessage = "Please enter both session code and your name.";
                return Page();
            }

            // Find the session
            var session = _sessionService.GetSessionByCode(SessionCode);

            if (session == null)
            {
                ErrorMessage = "Invalid session code. Please check and try again.";
                return Page();
            }

            // Check if session is full
            var currentPlayers = _playerService.GetPlayersInSession(session.Id);
            if (currentPlayers.Count >= session.MaxPlayers)
            {
                ErrorMessage = "This session is full.";
                return Page();
            }

            // Add player to session
            var player = _playerService.JoinSession(session.Id, PlayerName);

            // Redirect to lobby
            return RedirectToPage("/Lobby", new { sessionCode = SessionCode, playerId = player.Id });


        } //closing IActionResult OnPost()
    }
}