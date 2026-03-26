using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserService _userService;

        public RegisterModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var user = await _userService.RegisterEducatorAsync(Name, Email, Password);

                if (user == null)
                {
                    ErrorMessage = "Username or email already exists";
                    return Page();
                }

                // Clear any leftover student session data
                HttpContext.Session.Clear();

                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("EducatorName", user.Username);
                HttpContext.Session.SetString("Role", "Educator");

                return RedirectToPage("/EducatorDashboard");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message} | Inner: {ex.InnerException?.Message}";
                return Page();
            }
        }
    }
}
