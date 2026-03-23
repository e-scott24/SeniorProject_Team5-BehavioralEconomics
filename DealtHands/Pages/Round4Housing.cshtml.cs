using DealtHands.Models;
using DealtHands.Models.Cards;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class Round4HousingModel : PageModel
    {
        private readonly GameEngine _gameEngine;
        private readonly PlayerService _playerService;
        private readonly GameChangerService _gcService;
        private readonly SessionService _sessionService;

        public Round4HousingModel(GameEngine gameEngine, PlayerService playerService,
                                   GameChangerService gcService, SessionService sessionService)
        {
            _gameEngine = gameEngine;
            _playerService = playerService;
            _gcService = gcService;
            _sessionService = sessionService;
        }

        public Player Player { get; set; }
        public List<GameChangerCard> Cards { get; set; } = new();

        public void OnGet(int playerId)
        {
            Player = _playerService.GetPlayer(playerId);

            if (Player != null)
            {
                var session = _sessionService.GetSessionById(Player.SessionId);
                var difficulty = session?.Difficulty ?? "Medium";
                Cards = _gcService.GetCardsForRound(4, difficulty);
            }
        }

        // Returns JSON — JS shows the card overlay without a page reload
        public IActionResult OnPostSelectHousing(int playerId, string housingDescription,
                                                  decimal monthlyPayment, decimal? purchasePrice)
        {
            _gameEngine.RecordChoice(
                playerId: playerId,
                roundNumber: 4,
                roundType: "Housing",
                choiceDescription: housingDescription,
                monthlyCost: monthlyPayment,
                totalPrice: purchasePrice
            );

            return new JsonResult(new { success = true, playerId });
        }

        // Applies one card's effect — called per card click from the overlay
        public IActionResult OnPostApplyCard(int playerId, int cardId)
        {
            var player = _playerService.GetPlayer(playerId);
            if (player == null) return new JsonResult(new { success = false });

            var session = _sessionService.GetSessionById(player.SessionId);
            var difficulty = session?.Difficulty ?? "Medium";

            var cards = _gcService.GetCardsForRound(4, difficulty);
            var card = cards.FirstOrDefault(c => c.Id == cardId);

            if (card == null) return new JsonResult(new { success = false });

            _gameEngine.ApplyGameChanger(playerId, card, 4);

            return new JsonResult(new { success = true });
        }
    }
}
