// Name: Jason Black
// Date: 3/4/2026
//
// PageModel for the difficulty selection screen.
// Reads and writes the "difficulty" session key.
// Only "easy" is functional right now.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class DifficultySettingsModel : PageModel
    {
        // Exposed to the view so the correct card renders as active on load
        public string CurrentDifficulty { get; private set; } = "easy";

        public void OnGet()
        {
            CurrentDifficulty = HttpContext.Session.GetString("difficulty") ?? "easy";
        }

        public IActionResult OnPost(string difficulty)
        {
            // Guard against someone POSTing a difficulty that isn't built yet
            if (difficulty == "easy")
            {
                HttpContext.Session.SetString("difficulty", difficulty);
            }

            // Stay on this page so the player sees their selection reflected
            return RedirectToPage();
        }
    }
}
