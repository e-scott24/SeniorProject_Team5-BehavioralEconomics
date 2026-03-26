using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;

        public LoginModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userService.AuthenticateEducatorAsync(Email, Password);

            if (user == null)
            {
                ErrorMessage = "Invalid email or password";
                return Page();
            }

            // Clear any leftover student session data
            HttpContext.Session.Clear();

            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("EducatorName", user.Username);
            HttpContext.Session.SetString("Role", "Educator");

            return RedirectToPage("/EducatorDashboard");
        }
    }
}
