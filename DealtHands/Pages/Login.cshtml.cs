using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;
        private readonly IAuthenticationService _authService;

        public LoginModel(UserService userService, IAuthenticationService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // If already logged in as educator, redirect to dashboard
            if (_authService.IsEducator)
                return RedirectToPage("/EducatorDashboard");

            // If logged in as student, redirect to home
            if (_authService.IsStudent)
                return RedirectToPage("/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userService.AuthenticateEducatorAsync(Email, Password);

            if (user == null)
            {
                ErrorMessage = "Invalid email or password";
                return Page();
            }

            // Use the authentication service to set educator session
            _authService.SetEducatorSession(user.UserId, user.Username);

            return RedirectToPage("/EducatorDashboard");
        }
    }
}