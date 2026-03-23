using DealtHands.Models;
using DealtHands.Models.Cards;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class Round1CareerModel : PageModel
    {
        private readonly PlayerService _playerService;
        private readonly GameEngine _gameEngine;
        private readonly GameChangerService _gcService;
        private readonly SessionService _sessionService;

        public Round1CareerModel(PlayerService playerService, GameEngine gameEngine,
                                  GameChangerService gcService, SessionService sessionService)
        {
            _playerService = playerService;
            _gameEngine = gameEngine;
            _gcService = gcService;
            _sessionService = sessionService;
        }

        public Player Player { get; set; }

        // All career Game Changer cards — pre-loaded so the overlay
        // has everything it needs without an extra round trip
        public List<GameChangerCard> Cards { get; set; } = new();

        public void OnGet(int playerId)
        {
            Player = _playerService.GetPlayer(playerId);

            if (Player != null)
            {
                var session = _sessionService.GetSessionById(Player.SessionId);
                var difficulty = session?.Difficulty ?? "Medium";
                Cards = _gcService.GetCardsForRound(1, difficulty);
            }
        }

        // Called via fetch() when the player clicks a career button.
        // Records the choice and returns JSON — no redirect.
        // The page JS then shows the card overlay.
        public IActionResult OnPostSelectCareer(int playerId, string careerName,
                                                 decimal monthlySalary, decimal salary)
        {
            _gameEngine.RecordChoice(
                playerId: playerId,
                roundNumber: 1,
                roundType: "Career",
                choiceDescription: careerName,
                monthlyCost: 0,
                totalPrice: monthlySalary,
                annualSalary: salary
            );

            return new JsonResult(new { success = true, playerId });
        }

        // Called via fetch() when the player clicks Continue on each card.
        // Looks up card by Id — safe regardless of shuffle order.
        public IActionResult OnPostApplyCard(int playerId, int cardId)
        {
            var player = _playerService.GetPlayer(playerId);
            if (player == null) return new JsonResult(new { success = false });

            var session = _sessionService.GetSessionById(player.SessionId);
            var difficulty = session?.Difficulty ?? "Medium";

            var cards = _gcService.GetCardsForRound(1, difficulty);
            var card = cards.FirstOrDefault(c => c.Id == cardId);

            if (card == null) return new JsonResult(new { success = false });

            _gameEngine.ApplyGameChanger(playerId, card, 1);

            return new JsonResult(new { success = true });
        }
    }
}
