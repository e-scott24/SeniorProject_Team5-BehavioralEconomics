/*  Name: Jason Black
    Date: 3/20/2026
    Last Update: 3/21/2026

    PageModel for GameChanger.cshtml.
    Round 1 does not use this page — it handles cards inline via the overlay.
    Available for standalone card sequences on other rounds if needed.

    The correct file paths are the following:
    ...\DealtHands\DealtHands\Services\GameChangerService.cs
    ...\DealtHands\DealtHands\Services\GameEngine.cs
    ...\DealtHands\DealtHands\Models\GameChangerEvent.cs
    ...\DealtHands\DealtHands\Models\Cards\GameChangerCard.cs
    ...\DealtHands\DealtHands\Pages\Shared\GameChangerOverlayModel.cs
    ...\DealtHands\DealtHands\Pages\Shared\Cards\_GameChangerCard.cshtml
    ...\DealtHands\DealtHands\Pages\Shared\Cards\_GameChangerOverlay.cshtml
*/

using DealtHands.Models.Cards;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class GameChangerModel : PageModel
    {
        private readonly GameChangerService _gcService;
        private readonly GameEngine _gameEngine;
        private readonly PlayerService _playerService;
        private readonly SessionService _sessionService;

        public GameChangerModel(GameChangerService gcService, GameEngine gameEngine,
                                 PlayerService playerService, SessionService sessionService)
        {
            _gcService = gcService;
            _gameEngine = gameEngine;
            _playerService = playerService;
            _sessionService = sessionService;
        }

        public List<GameChangerCard> Cards { get; set; } = new();
        public int PlayerId { get; set; }
        public int Round { get; set; }
        public int NextRound { get; set; }

        // Default to empty string — assigned in OnGet via GetRoundPage()
        public string NextRoundPage { get; set; } = string.Empty;

        public IActionResult OnGet(int playerId, int round, int nextRound)
        {
            var player = _playerService.GetPlayer(playerId);
            if (player == null) return RedirectToPage("/Index");

            var session = _sessionService.GetSessionById(player.SessionId);
            var difficulty = session?.Difficulty ?? "Medium";

            PlayerId = playerId;
            Round = round;
            NextRound = nextRound;
            NextRoundPage = _gameEngine.GetRoundPage(nextRound);
            Cards = _gcService.GetCardsForRound(round, difficulty);

            return Page();
        }

        // POST — applies one card's effect when the player clicks Continue.
        // Looks up by cardId 
        public IActionResult OnPostApplyCard(int playerId, int cardId, int round, int nextRound)
        {
            var player = _playerService.GetPlayer(playerId);
            if (player == null) return new JsonResult(new { success = false });

            var session = _sessionService.GetSessionById(player.SessionId);
            var difficulty = session?.Difficulty ?? "Medium";

            var cards = _gcService.GetCardsForRound(round, difficulty);
            var card = cards.FirstOrDefault(c => c.Id == cardId);

            if (card == null) return new JsonResult(new { success = false });

            _gameEngine.ApplyGameChanger(playerId, card, round);

            return new JsonResult(new { success = true });
        }
    }
}
