/*  Name: Jason Black
   Date: 3/4/2026
   Last Update: 3/16/2026

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

        public FinancialsController(GameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var difficulty = HttpContext.Session.GetString("difficulty") ?? "easy";

            if (long.TryParse(HttpContext.Session.GetString("UserId"), out long userId) &&
                long.TryParse(HttpContext.Session.GetString("GameSessionId"), out long gameSessionId))
            {
                var state = await _gameSessionService.GetPlayerFinancialStateAsync(userId, gameSessionId);

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