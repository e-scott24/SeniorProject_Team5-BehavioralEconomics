using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class CreateSessionModel : PageModel
    {
        private readonly SessionService _sessionService;

        // Constructor injection - ASP.NET automatically provides the service
        public CreateSessionModel(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [BindProperty]
        public string SessionName { get; set; }

        [BindProperty]
        public string GameMode { get; set; }

        [BindProperty]
        public string Difficulty { get; set; }

        [BindProperty]
        public int MaxPlayers { get; set; } = 35;

        public void OnGet()
        {
            // Page loads
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            // Get educator ID from session (if needed)
            int? educatorId = HttpContext.Session.GetInt32("EducatorId");

            // Use service to create session
            var session = _sessionService.CreateSession(SessionName, GameMode, Difficulty, MaxPlayers);

            // Redirect with session code
            return RedirectToPage("/Lobby", new { sessionCode = session.Code });
        }
    }
}