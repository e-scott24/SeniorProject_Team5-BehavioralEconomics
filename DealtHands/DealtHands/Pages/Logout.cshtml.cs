using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthenticationService _authService;

        public LogoutModel(IAuthenticationService authService)
        {
            _authService = authService;
        }

        public IActionResult OnGet()
        {
            // Use authentication service to clear session
            _authService.ClearSession();
            return RedirectToPage("/Index");
        }
    }
}