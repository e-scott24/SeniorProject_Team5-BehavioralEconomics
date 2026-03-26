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

        public JoinSessionModel(GameSessionService gameSessionService, UserService userService,
                                SessionTracker sessionTracker)
        {
            _gameSessionService = gameSessionService;
            _userService = userService;
            _sessionTracker = sessionTracker;
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

            // Returning player — PlayerCode is their UserId
            if (!string.IsNullOrEmpty(PlayerCode))
            {
                if (!long.TryParse(PlayerCode, out long returningUserId))
                {
                    ErrorMessage = "Invalid player code.";
                    return Page();
                }

                var returningUser = await _userService.GetUserByIdAsync(returningUserId);
                if (returningUser == null)
                {
                    ErrorMessage = "Player code not found.";
                    return Page();
                }

                // Clear ALL existing session data and set student identity fresh
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("Role", "Student");
                HttpContext.Session.SetString("UserId", returningUserId.ToString());
                HttpContext.Session.SetString("PlayerName", returningUser.Username);
                HttpContext.Session.SetString("GameSessionId", session.GameSessionId.ToString());

                _sessionTracker.AddPlayer(session.GameSessionId, returningUserId);

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

            // Clear ALL existing session data and set student identity fresh
            HttpContext.Session.Clear();
            HttpContext.Session.SetString("Role", "Student");
            HttpContext.Session.SetString("UserId", student.UserId.ToString());
            HttpContext.Session.SetString("PlayerName", student.Username);
            HttpContext.Session.SetString("GameSessionId", session.GameSessionId.ToString());

            _sessionTracker.AddPlayer(session.GameSessionId, student.UserId);

            return RedirectToPage("/Lobby", new { sessionCode = SessionCode });
        }
    }
}
