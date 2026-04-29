using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserService _userService;
        private readonly IAuthenticationService _authService;

        public RegisterModel(UserService userService, IAuthenticationService authService)
        {
            _userService = userService;
            _authService = authService;
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
            if (string.IsNullOrWhiteSpace(Password)
                || Password.Length < 8
                || !Password.Any(char.IsLower)
                || !Password.Any(char.IsUpper)
                || !Password.Any(char.IsDigit)
                || !Password.Any(c => !char.IsLetterOrDigit(c)))
            {
                ErrorMessage = "Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a number, and a special character.";
                return Page();
            }

            try
            {
                var user = await _userService.RegisterEducatorAsync(Name, Email, Password);

                if (user == null)
                {
                    ErrorMessage = "Username or email already exists";
                    return Page();
                }

                // Use the authentication service to set educator session
                _authService.SetEducatorSession(user.UserId, user.Username);

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