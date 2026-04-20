/*  Name: Jason Black
   Date: 3/4/2026
   Last Update: 4/19/2026

   Temporary API controller that serves the player's financial starting state
   to the budget calculator via GET /api/financials.


   Calculator logic is in calculator.js.
   All markup/CSS is in _Calculator.cshtml. 
   Refer to FinancialsController.cs for the APIs. 
   Layout and open/close logic are in _Layout.cshtml.
   Program.cs was altered to support API controllers, session, and distributed memory cache.
   All icons are from https://heroicons.com/.

   The correct file paths are the following:
   ...\DealtHands\DealtHands\wwwroot\js\calculator.js
   ...\DealtHands\DealtHands\Pages\Shared\_Calculator.cshtml
   ...\DealtHands\DealtHands\Controllers\FinancialsController.cs
   ...\DealtHands\DealtHands\Pages\Shared\_Layout.cshtml
   ...\DealtHands\DealtHands\Program.cs
*/

using Microsoft.AspNetCore.Mvc;
using DealtHands.Services;

namespace DealtHands.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialsController : ControllerBase
    {
        private readonly GameSessionService _gameSessionService;
        private readonly IAuthenticationService _authService;

        public FinancialsController(GameSessionService gameSessionService, IAuthenticationService authService)
        {
            _gameSessionService = gameSessionService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Resolve difficulty from the player's GameSession record if available.
            // Fall back to session cookie, then to "easy" for educator calculator previews.
            string difficulty = "easy";

            long? userId = _authService.UserId;
            long? gameSessionId = _authService.GameSessionId;

            if (gameSessionId.HasValue)
            {
                var session = await _gameSessionService.GetSessionByIdAsync(gameSessionId.Value);
                if (session != null && !string.IsNullOrWhiteSpace(session.Difficulty))
                {
                    difficulty = session.Difficulty.ToLowerInvariant();
                }
            }
            else
            {
                // Session cookie fallback
                var cookieDifficulty = HttpContext.Session.GetString("difficulty");
                if (!string.IsNullOrWhiteSpace(cookieDifficulty))
                {
                    difficulty = cookieDifficulty.ToLowerInvariant();
                }
            }

            if (userId.HasValue && gameSessionId.HasValue)
            {
                var state = await _gameSessionService.GetPlayerFinancialStateAsync(userId.Value, gameSessionId.Value);

                return Ok(new
                {
                    difficulty,
                    monthlyIncome = state.MonthlyIncome,
                    checkingBalance = state.Available,
                    totalDebt = 0, // V2 schema does not track total debt separately
                    emergencyFundSaved = state.Available
                });
            }

            // Fallback if no player session (educator viewing calculator)
            return Ok(new
            {
                difficulty,
                monthlyIncome = 0,
                checkingBalance = 0,
                totalDebt = 0,
                emergencyFundSaved = 0
            });
        }
    }
}
