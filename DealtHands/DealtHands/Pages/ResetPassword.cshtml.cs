using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("NewPassword", ErrorMessage = "Passwords don't match")]
        public string ConfirmPassword { get; set; }

        public string Message { get; set; }
        public bool IsError { get; set; }

        public void OnGet()
        {
            // Validate that a token was provided
            if (string.IsNullOrWhiteSpace(Token))
            {
                Message = "Invalid reset link";
                IsError = true;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                IsError = true;
                return Page();
            }

            // Additional manual check (redundant with [Compare] but explicit)
            if (NewPassword != ConfirmPassword)
            {
                Message = "Passwords don't match";
                IsError = true;
                return Page();
            }

            try
            {
                // Attempt to reset the password
                bool success = await _userService.ResetPasswordWithTokenAsync(Token, NewPassword);

                if (success)
                {
                    // Redirect to login page with success message
                    TempData["SuccessMessage"] = "Password reset successfully. Please log in.";
                    return RedirectToPage("/Login");
                }

                // Token was invalid or expired
                Message = "Invalid or expired reset link";
                IsError = true;
                return Page();
            }
            catch (Exception)
            {
                // Log the exception in production
                Message = "An error occurred. Please try again later.";
                IsError = true;
                return Page();
            }
        }
    }
}