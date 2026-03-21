using DealtHands.Models;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class LobbyModel : PageModel
    {
        private readonly SessionService _sessionService;
        private readonly PlayerService _playerService;

        public LobbyModel(SessionService sessionService, PlayerService playerService)
        {
            _sessionService = sessionService;
            _playerService = playerService;
        }

        public Player Player { get; set; }

        public string SessionCode { get; set; }
        public Session Session { get; set; }
        public List<Player> Players { get; set; }
        public bool IsEducator { get; set; }
        public string SessionName { get; set; }
        public string GameMode { get; set; }
        public string Difficulty { get; set; }
        public int MaxPlayers { get; set; }

        public void OnGet(string sessionCode, int? playerId)
        {
            SessionCode = sessionCode;
            IsEducator = !playerId.HasValue;

            Session = _sessionService.GetSessionByCode(sessionCode);

            if (Session != null)
            {
                Players = _playerService.GetPlayersInSession(Session.Id);

                // Get current player if student
                if (playerId.HasValue)
                {
                    Player = _playerService.GetPlayer(playerId.Value);
                }
            }
        }

        // Fixed to redirect educator AND store educator's playerId
        public IActionResult OnPostStartGame(string sessionCode)
        {
            var session = _sessionService.GetSessionByCode(sessionCode);
            if (session == null) return RedirectToPage("/CreateSession");

            // Start the session
            _sessionService.StartSession(session.Id);


            //Educator stays on lobby, players will be redirected to Round 1 via JS
            return RedirectToPage("/Lobby", new { sessionCode = sessionCode });

            /*
            // Redirect educator to Round 1 (educator also needs to play)
            return RedirectToPage("/Round1Career", new { sessionCode = sessionCode });
            */
        }

        // API endpoint for fetching players
        public JsonResult OnGetGetPlayers(string sessionCode)
        {
            var session = _sessionService.GetSessionByCode(sessionCode);
            if (session == null) return new JsonResult(new List<object>());

            var players = _playerService.GetPlayersInSession(session.Id);
            return new JsonResult(players.Select(p => new { p.Name, p.Id }));
        }

        // Check if game has started
        public JsonResult OnGetCheckGameStarted(string sessionCode)
        {
            var session = _sessionService.GetSessionByCode(sessionCode);
            return new JsonResult(session?.IsStarted ?? false);
        }

        // Cancel Session handler
        public IActionResult OnPostCancelSession(string sessionCode)
        {
            var session = _sessionService.GetSessionByCode(sessionCode);
            if (session == null) return RedirectToPage("/Dashboard");
            else
            {
                _sessionService.CancelSession(session.Id);
            }
            return RedirectToPage("/Dashboard");

        }

        public JsonResult OnGetCheckSessionCancelled(string sessionCode)
        {
            var session = _sessionService.GetSessionByCode(sessionCode);
            return new JsonResult(session?.IsActive ?? true); // Return true if active, false if cancelled
        }



    }
}