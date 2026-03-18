using DealtHands.Models;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class Round4HousingModel : PageModel
    {
        private readonly GameEngine _gameEngine;
        private readonly PlayerService _playerService;

        public Round4HousingModel(GameEngine gameEngine, PlayerService playerService)
        {
            _gameEngine = gameEngine;
            _playerService = playerService;
        }

        public Player Player { get; set; }

        public void OnGet(int playerId)
        {
            Player = _playerService.GetPlayer(playerId);
        }

        public IActionResult OnPostSelectHousing(int playerId, string housingDescription, decimal monthlyPayment, decimal? purchasePrice)
        {
            // Record the choice
            _gameEngine.RecordChoice(
                playerId: playerId,
                roundNumber: 4,
                roundType: "Housing",
                choiceDescription: housingDescription,
                monthlyCost: monthlyPayment,
                totalPrice: purchasePrice
            );

            // Move to next round
            return RedirectToPage("/Round5Family", new { playerId = playerId });
        }
    }
}