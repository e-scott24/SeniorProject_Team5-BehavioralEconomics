using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserService _userService;

        public ResetPasswordModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string Message { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (NewPassword != ConfirmPassword)
            {
                Message = "Passwords don't match";
                return Page();
            }

            bool success = await _userService.ResetPasswordWithTokenAsync(Token, NewPassword);

            if (success)
                return RedirectToPage("/Login");

            Message = "Invalid or expired reset link";
            return Page();
        }
    }
}
