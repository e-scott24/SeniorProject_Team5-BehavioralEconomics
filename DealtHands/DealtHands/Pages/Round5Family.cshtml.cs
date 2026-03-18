using DealtHands.Models;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class Round5FamilyModel : PageModel
    {
        private readonly GameEngine _gameEngine;
        private readonly PlayerService _playerService;

        public Round5FamilyModel(GameEngine gameEngine, PlayerService playerService)
        {
            _gameEngine = gameEngine;
            _playerService = playerService;
        }

        public Player Player { get; set; }

        public void OnGet(int playerId)
        {
            Player = _playerService.GetPlayer(playerId);
        }

        public IActionResult OnPostSelectFamily(int playerId, string familyDescription, decimal monthlyPayment)
        {
            // Record the choice
            _gameEngine.RecordChoice(
                playerId: playerId,
                roundNumber: 5,
                roundType: "Family",
                choiceDescription: familyDescription,
                monthlyCost: monthlyPayment,
                totalPrice: null
            );
            
            // Move to results page
            return RedirectToPage("/Results", new { playerId = playerId });
        }
    }
}