using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;
using System.ComponentModel.DataAnnotations;

namespace DealtHands.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserService _userService;

        public ForgotPasswordModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        public bool TokenGenerated { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Generate password reset token
                Token = await _userService.GeneratePasswordResetTokenAsync(Email);

                if (Token != null)
                {
                    TokenGenerated = true;
                    // TODO: Email the token link to the educator
                    // For now, just display it on the page
                }
                else
                {
                    // Email not found - show error message
                    Message = "No account found with that email address.";
                }
            }
            catch (Exception)
            {
                // Log the exception in production
                Message = "An error occurred. Please try again later.";
                return Page();
            }

            return Page();
        }
    }
}