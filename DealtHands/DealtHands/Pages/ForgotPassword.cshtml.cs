using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly EducatorService _educatorService;

        public ForgotPasswordModel(EducatorService educatorService)
        {
            _educatorService = educatorService;
        }

        [BindProperty]
        public string Email { get; set; }

        public bool TokenGenerated { get; set; }
        public string Token { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            Token = _educatorService.GeneratePasswordResetToken(Email);

            if (Token != null)
            {
                TokenGenerated = true;
                // TODO: Email the token link to the educator
                // For now, just display it
            }

            return Page();
        }
    }
}