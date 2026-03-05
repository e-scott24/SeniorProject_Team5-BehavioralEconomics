/*  Name: Jason Black
    Date: 3/4/2026
    Last Update: 3/4/2026

    Temporary API controller that serves the player's financial starting state
    to the budget calculator via GET /api/financials.
*/

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FinancialsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        /*  Difficulty is read here and included in the response so calculator.js
            can select the correct income/expense arrays */
        var difficulty = HttpContext.Session.GetString("difficulty") ?? "easy";

        // Hardwired prefilled income (testing purposes)
        var monthlyIncome = HttpContext.Session.GetInt32("monthlyIncome") ?? 4500;

        // Hardwired account balance (testing purposes)
        var checkingBalance = HttpContext.Session.GetInt32("checkingBalance") ?? 3500;

        // Hardwired total debt (testing purposes)
        var totalDebt = HttpContext.Session.GetInt32("totalDebt") ?? 12000;

        // Hardwired emergency fund amount (testing purposes) - reflects money saved that can be saved and used when necessary 
        var emergencyFundSaved = HttpContext.Session.GetInt32("emergencyFundSaved") ?? 200;

        return Ok(new { difficulty, monthlyIncome, checkingBalance, totalDebt, emergencyFundSaved });
    }
}
