using DealtHands.Data;
using DealtHands.ModelsV2;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DealtHands.Pages
{
    public class LobbyModel : PageModel
    {
        private readonly GameSessionService _gameSessionService;
        private readonly UserService _userService;
        private readonly SessionTracker _sessionTracker;
        private readonly DealtHandsDbv2Context _context;
        private readonly IAuthenticationService _authService;

        public LobbyModel(GameSessionService gameSessionService, UserService userService,
                          SessionTracker sessionTracker, DealtHandsDbv2Context context,
                          IAuthenticationService authService)
        {
            _gameSessionService = gameSessionService;
            _userService = userService;
            _sessionTracker = sessionTracker;
            _context = context;
            _authService = authService;
        }

        public User Player { get; set; }
        public string SessionCode { get; set; }
        public GameSession Session { get; set; }
        public List<User> Players { get; set; } = new List<User>();
        public bool IsEducator { get; set; }

        public async Task<IActionResult> OnGetAsync(string sessionCode)
        {
            SessionCode = sessionCode;
            Session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (Session == null) return RedirectToPage("/Index");

            // Check role using authentication service
            IsEducator = _authService.IsEducator;

            if (IsEducator)
            {
                // Verify the educator owns this session
                if (!_authService.UserId.HasValue || Session.HostUserId != _authService.UserId.Value)
                    return RedirectToPage("/EducatorDashboard");

                // Ensure SessionCode is stored
                _authService.SetSessionCode(sessionCode);
            }
            else
            {
                // Verify the student is actually in this session
                if (!_authService.GameSessionId.HasValue || _authService.GameSessionId.Value != Session.GameSessionId)
                    return RedirectToPage("/JoinSession");
            }

            // Build player list from in-memory tracker
            var playerIds = _sessionTracker.GetPlayers(Session.GameSessionId);
            foreach (var pid in playerIds)
            {
                var user = await _userService.GetUserByIdAsync(pid);
                if (user != null) Players.Add(user);
            }

            // Load student's own record for the player code display
            if (!IsEducator && _authService.UserId.HasValue)
                Player = await _userService.GetUserByIdAsync(_authService.UserId.Value);

            return Page();
        }

        // Educator: start the game and open Round 1
        public async Task<IActionResult> OnPostStartGameAsync(string sessionCode)
        {
            if (!_authService.IsEducator)
                return RedirectToPage("/Index");

            if (!_authService.UserId.HasValue)
                return RedirectToPage("/Login");

            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null) return RedirectToPage("/CreateSession");
            if (session.HostUserId != _authService.UserId.Value) return RedirectToPage("/EducatorDashboard");

            _authService.SetSessionCode(sessionCode);

            var connectedUserIds = _sessionTracker.GetPlayers(session.GameSessionId);
            if (connectedUserIds.Any())
                await _gameSessionService.OpenRoundAsync(session.GameSessionId, connectedUserIds);

            await _gameSessionService.StartSessionAsync(session.GameSessionId);

            return RedirectToPage("/Lobby", new { sessionCode = sessionCode });
        }

        // Educator: close current round and open next
        public async Task<IActionResult> OnPostCloseRoundAsync(string sessionCode)
        {
            if (!_authService.IsEducator)
                return RedirectToPage("/Index");

            if (!_authService.UserId.HasValue)
                return RedirectToPage("/Login");

            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null) return RedirectToPage("/EducatorDashboard");
            if (session.HostUserId != _authService.UserId.Value) return RedirectToPage("/EducatorDashboard");

            var round = await _gameSessionService.GetOpenRoundAsync(session.GameSessionId);
            if (round != null)
            {
                await _gameSessionService.CloseRoundAsync(round.GameRoundId);

                session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);

                if (session.CurrentRoundNumber <= 5)
                {
                    var connectedUserIds = _sessionTracker.GetPlayers(session.GameSessionId);
                    await _gameSessionService.OpenRoundAsync(session.GameSessionId, connectedUserIds);
                }
                else
                {
                    // Game complete - redirect to session report instead of dashboard
                    await _gameSessionService.EndSessionAsync(session.GameSessionId);
                    return RedirectToPage("/SessionReport", new { sessionId = session.GameSessionId });
                }
            }

            return RedirectToPage("/Lobby", new { sessionCode = sessionCode });
        }

        // Educator: pause
        public async Task<IActionResult> OnPostPauseSessionAsync(string sessionCode)
        {
            if (!_authService.IsEducator)
                return RedirectToPage("/Index");

            if (!_authService.UserId.HasValue)
                return RedirectToPage("/Login");

            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null) return RedirectToPage("/EducatorDashboard");
            if (session.HostUserId != _authService.UserId.Value) return RedirectToPage("/EducatorDashboard");

            await _gameSessionService.PauseSessionAsync(session.GameSessionId);
            return RedirectToPage("/EducatorDashboard");
        }

        // Educator: resume
        public async Task<IActionResult> OnPostResumeSessionAsync(string sessionCode)
        {
            if (!_authService.IsEducator)
                return RedirectToPage("/Index");

            if (!_authService.UserId.HasValue)
                return RedirectToPage("/Login");

            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null) return RedirectToPage("/EducatorDashboard");
            if (session.HostUserId != _authService.UserId.Value) return RedirectToPage("/EducatorDashboard");

            await _gameSessionService.ResumeSessionAsync(session.GameSessionId);
            return RedirectToPage("/Lobby", new { sessionCode = sessionCode });
        }

        // Educator: cancel
        public async Task<IActionResult> OnPostCancelSessionAsync(string sessionCode)
        {
            if (!_authService.IsEducator)
                return RedirectToPage("/Index");

            if (!_authService.UserId.HasValue)
                return RedirectToPage("/Login");

            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null) return RedirectToPage("/EducatorDashboard");
            if (session.HostUserId != _authService.UserId.Value) return RedirectToPage("/EducatorDashboard");

            await _gameSessionService.EndSessionAsync(session.GameSessionId);
            return RedirectToPage("/EducatorDashboard");
        }

        // --- Polling endpoints ---

        public async Task<JsonResult> OnGetGetPlayersAsync(string sessionCode)
        {
            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null) return new JsonResult(new List<object>());

            var playerIds = _sessionTracker.GetPlayers(session.GameSessionId);
            var players = new List<object>();
            foreach (var pid in playerIds)
            {
                var user = await _userService.GetUserByIdAsync(pid);
                if (user != null)
                    players.Add(new { Name = user.Username, Id = user.PlayerCode });
            }
            return new JsonResult(players);
        }

        // Students poll this to know when to redirect to /Round
        public async Task<JsonResult> OnGetCheckGameStartedAsync(string sessionCode)
        {
            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            return new JsonResult(session?.Status == "InProgress");
        }

        // Students poll this to detect pause or cancellation
        public async Task<JsonResult> OnGetCheckSessionStatusAsync(string sessionCode)
        {
            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            return new JsonResult(new
            {
                status = session?.Status ?? "Unknown",
                isActive = session?.IsActive ?? false
            });
        }

        // Educator control panel polls this
        public async Task<JsonResult> OnGetRoundStatusAsync(string sessionCode)
        {
            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null) return new JsonResult(null);

            var round = await _gameSessionService.GetOpenRoundAsync(session.GameSessionId);
            if (round == null)
                return new JsonResult(new { roundOpen = false });

            // CardId != null identifies RoundCard UGC rows � GameChanger rows have CardId = null
            var totalAssigned = await _context.Ugcs
                .CountAsync(u => u.GameRoundId == round.GameRoundId && u.CardId != null);

            var totalSubmitted = await _context.Ugcs
                .CountAsync(u => u.GameRoundId == round.GameRoundId
                              && u.CardId != null
                              && u.SubmittedAt != null);

            return new JsonResult(new
            {
                roundOpen = true,
                roundNumber = round.RoundNumber,
                roundType = round.RoundType,
                submitted = totalSubmitted,
                total = totalAssigned,
                allSubmitted = totalAssigned > 0 && totalSubmitted >= totalAssigned
            });
        }
    }
}