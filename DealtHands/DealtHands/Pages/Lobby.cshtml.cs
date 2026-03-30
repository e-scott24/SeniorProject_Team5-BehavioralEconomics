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

        public LobbyModel(GameSessionService gameSessionService, UserService userService,
                          SessionTracker sessionTracker, DealtHandsDbv2Context context)
        {
            _gameSessionService = gameSessionService;
            _userService = userService;
            _sessionTracker = sessionTracker;
            _context = context;
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

            // Role is set at login (Educator) or JoinSession (Student) — never inferred
            IsEducator = HttpContext.Session.GetString("Role") == "Educator";

            if (IsEducator)
            {
                // Verify the educator owns this session
                if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long hostId)
                    || Session.HostUserId != hostId)
                    return RedirectToPage("/EducatorDashboard");

                // Ensure SessionCode is always in session for the educator
                HttpContext.Session.SetString("SessionCode", sessionCode);
            }
            else
            {
                // Verify the student is actually in this session
                if (!long.TryParse(HttpContext.Session.GetString("GameSessionId"), out long studentSessionId)
                    || studentSessionId != Session.GameSessionId)
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
            if (!IsEducator && long.TryParse(HttpContext.Session.GetString("UserId"), out long uid))
                Player = await _userService.GetUserByIdAsync(uid);

            return Page();
        }



        /*
        // Educator: start the game and open Round 1
        public async Task<IActionResult> OnPostStartGameAsync(string sessionCode)
        {
            if (HttpContext.Session.GetString("Role") != "Educator")
                return RedirectToPage("/Index");
            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long hostId))
                return RedirectToPage("/Login");

            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null) return RedirectToPage("/CreateSession");
            if (session.HostUserId != hostId) return RedirectToPage("/EducatorDashboard");

            HttpContext.Session.SetString("SessionCode", sessionCode);

            await _gameSessionService.StartSessionAsync(session.GameSessionId);

            var connectedUserIds = _sessionTracker.GetPlayers(session.GameSessionId);
            if (connectedUserIds.Any())
                await _gameSessionService.OpenRoundAsync(session.GameSessionId, connectedUserIds);

            return RedirectToPage("/Lobby", new { sessionCode = sessionCode });
        }
        */

        // DEBUG - educator start session
        public async Task<IActionResult> OnPostStartGameAsync(string sessionCode)
        {
            // Diagnostic logging
            var role = HttpContext.Session.GetString("Role");
            var userIdStr = HttpContext.Session.GetString("UserId");

            TempData["Debug"] = $"Role: '{role}' | UserId: '{userIdStr}' | SessionCode: '{sessionCode}'";

            if (HttpContext.Session.GetString("Role") != "Educator")
            {
                TempData["Debug"] += " | FAILED: Role check";
                return RedirectToPage("/Index");
            }

            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long hostId))
            {
                TempData["Debug"] += " | FAILED: UserId parse";
                return RedirectToPage("/Login");
            }

            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null)
            {
                TempData["Debug"] += " | FAILED: Session not found";
                return RedirectToPage("/CreateSession");
            }

            if (session.HostUserId != hostId)
            {
                TempData["Debug"] += $" | FAILED: HostUserId mismatch ({session.HostUserId} vs {hostId})";
                return RedirectToPage("/EducatorDashboard");
            }

            HttpContext.Session.SetString("SessionCode", sessionCode);

            await _gameSessionService.StartSessionAsync(session.GameSessionId);

            var connectedUserIds = _sessionTracker.GetPlayers(session.GameSessionId);
            if (connectedUserIds.Any())
                await _gameSessionService.OpenRoundAsync(session.GameSessionId, connectedUserIds);

            TempData["Debug"] += " | SUCCESS";
            return RedirectToPage("/Lobby", new { sessionCode = sessionCode });
        }



        // Educator: close current round and open next
        public async Task<IActionResult> OnPostCloseRoundAsync(string sessionCode)
        {
            if (HttpContext.Session.GetString("Role") != "Educator")
                return RedirectToPage("/Index");
            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long hostId))
                return RedirectToPage("/Login");

            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null) return RedirectToPage("/EducatorDashboard");
            if (session.HostUserId != hostId) return RedirectToPage("/EducatorDashboard");

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
                    await _gameSessionService.EndSessionAsync(session.GameSessionId);
                    return RedirectToPage("/EducatorDashboard");
                }
            }

            return RedirectToPage("/Lobby", new { sessionCode = sessionCode });
        }

        // Educator: pause
        public async Task<IActionResult> OnPostPauseSessionAsync(string sessionCode)
        {
            if (HttpContext.Session.GetString("Role") != "Educator")
                return RedirectToPage("/Index");
            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long hostId))
                return RedirectToPage("/Login");

            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null) return RedirectToPage("/EducatorDashboard");
            if (session.HostUserId != hostId) return RedirectToPage("/EducatorDashboard");

            await _gameSessionService.PauseSessionAsync(session.GameSessionId);
            return RedirectToPage("/EducatorDashboard");
        }

        // Educator: resume
        public async Task<IActionResult> OnPostResumeSessionAsync(string sessionCode)
        {
            if (HttpContext.Session.GetString("Role") != "Educator")
                return RedirectToPage("/Index");
            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long hostId))
                return RedirectToPage("/Login");

            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null) return RedirectToPage("/EducatorDashboard");
            if (session.HostUserId != hostId) return RedirectToPage("/EducatorDashboard");

            await _gameSessionService.ResumeSessionAsync(session.GameSessionId);
            return RedirectToPage("/Lobby", new { sessionCode = sessionCode });
        }

        // Educator: cancel
        public async Task<IActionResult> OnPostCancelSessionAsync(string sessionCode)
        {
            if (HttpContext.Session.GetString("Role") != "Educator")
                return RedirectToPage("/Index");
            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long hostId))
                return RedirectToPage("/Login");

            var session = await _gameSessionService.GetSessionByJoinCodeAsync(sessionCode);
            if (session == null) return RedirectToPage("/EducatorDashboard");
            if (session.HostUserId != hostId) return RedirectToPage("/EducatorDashboard");

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
                    players.Add(new { Name = user.Username, Id = user.UserId });
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

            var totalAssigned = await _context.Ugcs
                .CountAsync(u => u.GameRoundId == round.GameRoundId
                              && u.Card.CardType == "RoundCard");

            var totalSubmitted = await _context.Ugcs
                .CountAsync(u => u.GameRoundId == round.GameRoundId
                              && u.SubmittedAt != null
                              && u.Card.CardType == "RoundCard");

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