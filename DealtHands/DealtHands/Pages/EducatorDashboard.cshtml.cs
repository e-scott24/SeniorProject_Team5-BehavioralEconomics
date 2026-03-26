using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;
using DealtHands.ModelsV2;

namespace DealtHands.Pages
{
    public class EducatorDashboardModel : PageModel
    {
        private readonly UserService _userService;

        public EducatorDashboardModel(UserService userService)
        {
            _userService = userService;
        }

        public List<GameSession> ActiveSessions { get; set; } = new List<GameSession>();
        public List<GameSession> CompletedSessions { get; set; } = new List<GameSession>();

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("Role") != "Educator")
                return RedirectToPage("/Login");

            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long userId))
                return RedirectToPage("/Login");

            var allSessions = await _userService.GetEducatorSessionsAsync(userId);

            // Active = anything not completed and still active
            ActiveSessions = allSessions.Where(s => s.Status != "Completed" && s.IsActive).ToList();

            // Completed = finished or manually cancelled
            CompletedSessions = allSessions.Where(s => s.Status == "Completed" || !s.IsActive).ToList();

            return Page();
        }
    }
}