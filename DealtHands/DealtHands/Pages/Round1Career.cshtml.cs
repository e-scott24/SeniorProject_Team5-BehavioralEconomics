using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DealtHands.Services;
using DealtHands.Models;

namespace DealtHands.Pages
{
    public class Round1CareerModel : PageModel
    {
        private readonly PlayerService _playerService;
        private readonly GameEngine _gameEngine;

        public Round1CareerModel(PlayerService playerService, GameEngine gameEngine)
        {
            _playerService = playerService;
            _gameEngine = gameEngine;
        }

        public Player Player { get; set; }

        public void OnGet(int playerId)
        {
            Player = _playerService.GetPlayer(playerId);
        }

        public IActionResult OnPostSelectCareer(int playerId, string careerName, decimal monthlySalary, decimal salary)
        {
            _gameEngine.RecordChoice(
                playerId: playerId,
                roundNumber: 1,
                roundType: "Career",
                choiceDescription: careerName,
                monthlyCost: 0,
                totalPrice: monthlySalary
               
            );

            var player = _playerService.GetPlayer(playerId);
            player.Salary = salary;

            return RedirectToPage("/Round2Loans", new { playerId = playerId });
        }
    }
}