using DealtHands.Models;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class Round2LoansModel : PageModel
    {
        private readonly GameEngine _gameEngine;
        private readonly PlayerService _playerService;

        public Round2LoansModel(GameEngine gameEngine, PlayerService playerService)
        {
            _gameEngine = gameEngine;
            _playerService = playerService;
        }

        public Player Player { get; set; }

        public void OnGet(int playerId)
        {
            Player = _playerService.GetPlayer(playerId);
        }

        

        public IActionResult OnPostSelectLoan(int playerId, string loanDescription, decimal monthlyPayment, decimal totalDebt)
        {
            _gameEngine.RecordChoice(
                playerId: playerId,
                roundNumber: 2,
                roundType: "Loans",
                choiceDescription: loanDescription,
                monthlyCost: monthlyPayment,
                totalPrice: totalDebt
            );

            // Update player's debt
            var player = _playerService.GetPlayer(playerId);
            player.TotalDebt += totalDebt;

            return RedirectToPage("/Round3Transportation", new { playerId = playerId });
        }

    }
}