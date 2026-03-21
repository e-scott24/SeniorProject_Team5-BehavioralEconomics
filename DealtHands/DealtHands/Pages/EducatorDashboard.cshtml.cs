using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;
using DealtHands.Models;

namespace DealtHands.Pages
{
    public class EducatorDashboardModel : PageModel
    {
        private readonly EducatorService _educatorService;

        public EducatorDashboardModel(EducatorService educatorService)
        {
            _educatorService = educatorService;
        }

        public List<Session> ActiveSessions { get; set; } = new List<Session>();
        public List<Session> CompletedSessions { get; set; } = new List<Session>();

        public IActionResult OnGet()
        {
            // Check if educator is logged in
            int? educatorId = HttpContext.Session.GetInt32("EducatorId");

            if (!educatorId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            // Get all sessions for this educator
            var allSessions = _educatorService.GetEducatorSessions(educatorId.Value);

            // Split into active and completed
            ActiveSessions = allSessions.Where(s => !s.IsCompleted && s.IsActive).ToList();
            CompletedSessions = allSessions.Where(s => s.IsCompleted || !s.IsActive).ToList();

            return Page();
        }
    }
}