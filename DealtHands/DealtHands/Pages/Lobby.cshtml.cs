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
        public string SessionName { get; set; }
        public string GameMode { get; set; }
        public string Difficulty { get; set; }
        public int MaxPlayers { get; set; }

        public void OnGet(string sessionCode, int? playerId)
        {
            SessionCode = sessionCode;

            // Get session details
            Session = _sessionService.GetSessionByCode(sessionCode);

            if (Session != null)
            {
                // Get all players in session
                Players = _playerService.GetPlayersInSession(Session.Id);
            }
        }
    }
}