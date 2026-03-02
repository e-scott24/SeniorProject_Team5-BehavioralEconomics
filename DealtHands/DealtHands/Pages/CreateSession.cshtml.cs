using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class CreateSessionModel : PageModel
    {
        [BindProperty]
        public string SessionName { get; set; }
        
        /*
        [BindProperty]
        public string PlayerName { get; set; }
        */

        [BindProperty]
        public string GameMode { get; set; }
        [BindProperty]
        public string Difficulty { get; set; }
        [BindProperty]
        public int MaxPlayers { get; set; } = 35; // Default to 35 players


        public void OnGet()
        {
            // Called when page is first loaded
        }


        public IActionResult OnPost()
        {
            //Check if all fields are filled out correctly
            if (string.IsNullOrEmpty(SessionName) 
                || string.IsNullOrEmpty(GameMode) 
                || string.IsNullOrEmpty(Difficulty) 
                || MaxPlayers <= 0 || MaxPlayers > 50)
            {
                ModelState.AddModelError(string.Empty, "Please fill in all fields correctly.");
                return Page();
            }


            //Generate simple 5-digit session code
            Random random = new Random();
            string sessionCode = random.Next(10000, 99999).ToString();

            // In a real application, you would save the session details to a database here,
            // For now, we will just redirect to the JoinSession page with the session code and store it in TempData
            TempData["SessionName"] = SessionName;
            TempData["GameMode"] = GameMode;
            TempData["Difficulty"] = Difficulty;
            TempData["MaxPlayers"] = MaxPlayers;

            // Redirect to the Lobby page with the session code
            return RedirectToPage("/Lobby", new { sessionCode = sessionCode });


            /* VisualStudio IntelliSense code...
            // ------

            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            // Here you would typically create a new session in your database
            // using the provided SessionName, GameMode, Difficulty, and MaxPlayers.
            // For example:
            // var session = new GameSession
            // {
            //     Name = SessionName,
            //     GameMode = GameMode,
            //     Difficulty = Difficulty,
            //     MaxPlayers = MaxPlayers
            // };
            // _dbContext.GameSessions.Add(session);
            // _dbContext.SaveChanges();
            // After creating the session, redirect to a page where players can join.
            return RedirectToPage("/JoinSession", new { sessionName = SessionName });
            
            // ------
            */

        } // closing IActionResult OnPost()

    } // closing class CreateSessionModel

} // closing namespace DealtHands.Pages
