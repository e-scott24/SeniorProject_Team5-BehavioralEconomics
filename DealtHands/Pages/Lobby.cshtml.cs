using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class LobbyModel : PageModel
    {
        public string SessionCode { get; set; }
        public string SessionName { get; set; }
        public string GameMode { get; set; }
        public string Difficulty { get; set; }
        public int MaxPlayers { get; set; }

        public void OnGet(string sessionCode)
        {
            // Receive session code from URL parameter
            SessionCode = sessionCode;

            // Get other session details from TempData
            SessionName = TempData["SessionName"]?.ToString() ?? "Unknown Session";
            GameMode = TempData["GameMode"]?.ToString() ?? "Random";
            Difficulty = TempData["Difficulty"]?.ToString() ?? "Medium";
            MaxPlayers = TempData["MaxPlayers"] != null ? (int)TempData["MaxPlayers"] : 35;
        }
    }
}