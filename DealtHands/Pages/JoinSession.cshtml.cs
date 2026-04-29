using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class JoinSessionModel : PageModel
    {
        private readonly GameSessionService _gameSessionService;
        private readonly UserService _userService;
        private readonly SessionTracker _sessionTracker;
        private readonly IAuthenticationService _authService;

        public JoinSessionModel(GameSessionService gameSessionService, UserService userService,
                                SessionTracker sessionTracker, IAuthenticationService authService)
        {
            _gameSessionService = gameSessionService;
            _userService = userService;
            _sessionTracker = sessionTracker;
            _authService = authService;
        }

        [BindProperty]
        public string SessionCode { get; set; }

        [BindProperty]
        public string PlayerName { get; set; }

        // Player code = the student's UserId from a previous join
        [BindProperty]
        public string PlayerCode { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(SessionCode))
            {
                ErrorMessage = "Please enter a session code.";
                return Page();
            }

            var session = await _gameSessionService.GetSessionByJoinCodeAsync(SessionCode);

            if (session == null)
            {
                ErrorMessage = "Invalid session code.";
                return Page();
            }

            if (session.Status != "Waiting" && session.Status != "Paused")
            {
                ErrorMessage = "This session has already started or is no longer accepting players.";
                return Page();
            }

            // Returning player — PlayerCode is their 6-digit player code
            if (!string.IsNullOrEmpty(PlayerCode))
            {
                if (!int.TryParse(PlayerCode, out int code))
                {
                    ErrorMessage = "Invalid player code.";
                    return Page();
                }

                var returningUser = await _userService.GetUserByPlayerCodeAsync(code);
                if (returningUser == null)
                {
                    ErrorMessage = "Player code not found.";
                    return Page();
                }

                _authService.SetStudentSession(returningUser.UserId, returningUser.Username, session.GameSessionId);
                _sessionTracker.AddPlayer(session.GameSessionId, returningUser.UserId);

                return RedirectToPage("/Lobby", new { sessionCode = SessionCode });
            }

            // New player — only allowed on Waiting sessions
            if (session.Status == "Paused")
            {
                ErrorMessage = "This session is paused. Use your Player Code to rejoin.";
                return Page();
            }

            if (string.IsNullOrEmpty(PlayerName))
            {
                ErrorMessage = "Please enter your name.";
                return Page();
            }

            int currentCount = _sessionTracker.GetPlayerCount(session.GameSessionId);
            if (session.Game?.MaxPlayers.HasValue == true && currentCount >= session.Game.MaxPlayers.Value)
            {
                ErrorMessage = "This session is full.";
                return Page();
            }

            var student = await _userService.CreateOrGetStudentAsync(PlayerName);

            // Use authentication service to set student session
            _authService.SetStudentSession(student.UserId, student.Username, session.GameSessionId);

            _sessionTracker.AddPlayer(session.GameSessionId, student.UserId);

            return RedirectToPage("/Lobby", new { sessionCode = SessionCode });
        }
    }
}