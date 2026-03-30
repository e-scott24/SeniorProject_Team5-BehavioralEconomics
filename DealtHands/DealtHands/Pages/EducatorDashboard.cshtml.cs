using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;
using DealtHands.ModelsV2;

namespace DealtHands.Pages
{
    public class EducatorDashboardModel : PageModel
    {
        private readonly UserService _userService;
        private readonly IAuthenticationService _authService;

        public EducatorDashboardModel(UserService userService, IAuthenticationService authService)
        {
            _userService = userService;
            _authService = authService;
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
            CompletedSessions = allSessions.Where(s => s.Status == "Completed" || !s.IsActive).ToList();

            return Page();
        }
    }
}