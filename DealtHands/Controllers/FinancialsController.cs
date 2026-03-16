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
