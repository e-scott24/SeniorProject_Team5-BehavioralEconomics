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

        public IActionResult OnPostSelectCareer(int playerId, string careerName, decimal salary)
        {
            // Record the choice
            _gameEngine.RecordChoice(
                playerId: playerId,
                roundNumber: 2,
                roundType: "Loans",
                choiceDescription: careerName,
                monthlyCost: 0,
                totalPrice: salary
            );

            // Move to next round
            return RedirectToPage("/Round3Transportation", new { playerId = playerId });
        }
    }
}