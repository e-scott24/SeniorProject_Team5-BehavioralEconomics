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


        public string SessionCode { get; set; }
        public Session Session { get; set; }
        public List<Player> Players { get; set; }
        public bool IsEducator { get; set; } // Use this to determine if the current user is the educator (session creator)
        public string SessionName { get; set; }
        public string GameMode { get; set; }
        public string Difficulty { get; set; }
        public int MaxPlayers { get; set; }

        public void OnGet(string sessionCode, int? playerId)
        {
            SessionCode = sessionCode;
            IsEducator = !playerId.HasValue; // If no playerId is provided, we assume the user is the educator (session creator)

            // Get session details
            Session = _sessionService.GetSessionByCode(sessionCode);

            if (Session != null)
            {
                // Get all players in session
                Players = _playerService.GetPlayersInSession(Session.Id);
            }
        } //closing void OnGet()

        // POST method to start the game
        public IActionResult OnPostStartGame(string sessionCode)
        {
            var session = _sessionService.GetSessionByCode(sessionCode);
            if (session == null)
            {
                return RedirectToPage("/CreateSession"); // Session not found
            }

            // Start the session
            _sessionService.StartSession(session.Id);

            // Redirect the creator to the first round
            var firstPlayer = Players?.FirstOrDefault();
            if (firstPlayer == null)
            {
                // No players yet, just redirect to lobby again
                return RedirectToPage("/Lobby", new { sessionCode = sessionCode });
            }

            return RedirectToPage("/Round", new { playerId = firstPlayer.Id });
        } // closing IActionResult OnPostStartGame()

        public JsonResult OnGetGetPlayers(string sessionCode)
        {
            var session = _sessionService.GetSessionByCode(sessionCode);
            if (session == null) return new JsonResult(new List<object>());

            var players = _playerService.GetPlayersInSession(session.Id);
            return new JsonResult(players.Select(p => new { p.Name, p.Id }));
        }

    }
}