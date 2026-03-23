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

using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;

namespace DealtHands.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialsController : ControllerBase
    {
        private readonly PlayerService _playerService;

        public FinancialsController(PlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpGet]
        public IActionResult Get(int? playerId)
        {
            var difficulty = HttpContext.Session.GetString("difficulty") ?? "easy";

            if (playerId.HasValue)
            {
                var player = _playerService.GetPlayer(playerId.Value);
                if (player != null)
                {
                    return Ok(new
                    {
                        difficulty,
                        monthlyIncome = player.MonthlyIncome,
                        checkingBalance = player.Balance,     // ← was player.Savings, now player.Balance
                        totalDebt = player.TotalDebt,
                        emergencyFundSaved = player.Savings    // Savings stays as the emergency fund
                    });
                }
            }

            // Fallback if no player (educator viewing calculator)
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
