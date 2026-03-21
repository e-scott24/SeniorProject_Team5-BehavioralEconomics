using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;

namespace DealtHands.Pages
{
    public class LoginModel : PageModel
    {
        private readonly EducatorService _educatorService;

        public LoginModel(EducatorService educatorService)
        {
            _educatorService = educatorService;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            var educator = _educatorService.Login(Email, Password);

            if (educator == null)
            {
                ErrorMessage = "Invalid email or password";
                return Page();
            }

            // Store educator ID in session
            HttpContext.Session.SetInt32("EducatorId", educator.Id);
            HttpContext.Session.SetString("EducatorName", educator.Name);

            //return RedirectToPage("/CreateSession");
            return RedirectToPage("/EducatorDashboard");
        }
    }
}