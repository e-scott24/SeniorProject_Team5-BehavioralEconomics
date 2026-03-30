using DealtHands.ModelsV2;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DealtHands.Data;

namespace DealtHands.Pages
{
    public class EducatorDashboardModel : PageModel
    {
        private readonly UserService _userService;
        private readonly IAuthenticationService _authService;
        private readonly DealtHandsDbv2Context _context;

        public EducatorDashboardModel(UserService userService, IAuthenticationService authService,
                                      DealtHandsDbv2Context context)
        {
            _userService = userService;
            _authService = authService;
            _context = context;
        }

        public List<GameSession> ActiveSessions { get; set; } = new List<GameSession>();
        public List<GameSession> CompletedSessions { get; set; } = new List<GameSession>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is authenticated as educator
            if (!_authService.IsEducator)
                return RedirectToPage("/Login");

            if (!_authService.UserId.HasValue)
                return RedirectToPage("/Login");

            var allSessions = await _userService.GetEducatorSessionsAsync(_authService.UserId.Value);

            // Active = anything not completed and still active
            ActiveSessions = allSessions.Where(s => s.Status != "Completed" && s.IsActive).ToList();

            // Completed = finished or manually cancelled
            CompletedSessions = allSessions.Where(s => s.Status == "Completed" && s.IsActive).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostArchiveSessionAsync(long sessionId)
        {
            if (!_authService.IsEducator || !_authService.UserId.HasValue)
                return RedirectToPage("/Login");

            var session = await _context.GameSessions.FindAsync(sessionId);
            if (session == null || session.HostUserId != _authService.UserId.Value)
                return RedirectToPage("/EducatorDashboard");

            session.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToPage("/EducatorDashboard");
        }
    }
}