using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;
using DealtHands.Models;

namespace DealtHands.Pages
{
    public class Round3TransportationModel : PageModel
    {
        private readonly GameEngine _gameEngine;
        private readonly PlayerService _playerService;
        
        public Round3TransportationModel(GameEngine gameEngine, PlayerService playerService)
        {
            _gameEngine = gameEngine;
            _playerService = playerService;
        }
        
        public Player Player { get; set; }
        
        public void OnGet(int playerId)
        {
            Player = _playerService.GetPlayer(playerId);
        }

        public IActionResult OnPostSelectVehicle(int playerId, string vehicleDescription, decimal monthlyPayment, decimal purchasePrice)
        {
            // Record choices
            _gameEngine.RecordChoice(
                playerId: playerId,
                roundNumber: 3,
                roundType: "Transportation",
                choiceDescription: vehicleDescription,
                monthlyCost: monthlyPayment,
                totalPrice: purchasePrice
            );

            // Move to next round
            return RedirectToPage("/Round4Housing", new { playerId = playerId });
        }
    }
}