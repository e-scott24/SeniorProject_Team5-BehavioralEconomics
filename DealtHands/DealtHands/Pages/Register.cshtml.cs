using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly EducatorService _educatorService;

        public RegisterModel(EducatorService educatorService)
        {
            _educatorService = educatorService;
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
                // Use the async service method which returns null if duplicate
                var user = await _educatorService.RegisterEducatorAsync(Name, Email, Password);

                if (user == null)
                {
                    ErrorMessage = "Username or email already exists";
                    return Page();
                }

                // Auto-login after registration
                HttpContext.Session.SetInt32("EducatorId", user.Id);
                HttpContext.Session.SetString("EducatorName", user.Name);

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