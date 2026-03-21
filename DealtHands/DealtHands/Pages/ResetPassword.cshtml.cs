using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DealtHands.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly EducatorService _educatorService;

        public ResetPasswordModel(EducatorService educatorService)
        {
            _educatorService = educatorService;
        }

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string Message { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (NewPassword != ConfirmPassword)
            {
                Message = "Passwords don't match";
                return Page();
            }

            bool success = _educatorService.ResetPassword(Token, NewPassword);

            if (success)
            {
                return RedirectToPage("/Login");
            }

            Message = "Invalid or expired reset link";
            return Page();
        }
    }
}
