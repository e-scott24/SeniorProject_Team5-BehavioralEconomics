using DealtHands.Models;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class RoundModel : PageModel
    {
        private readonly GameEngine _gameEngine;
        private readonly PlayerService _playerService;
        private readonly SessionService _sessionService;

        public RoundModel(GameEngine gameEngine, PlayerService playerService, SessionService sessionService)
        {
            _gameEngine = gameEngine;
            _playerService = playerService;
            _sessionService = sessionService;
        }

        [BindProperty(SupportsGet = true)]
        public int PlayerId { get; set; }

        public Player Player { get; set; }
        public RoundConfig Round { get; set; }
        public string WaitingMessage { get; set; }

        public IActionResult OnGet()
        {
            Player = _playerService.GetPlayer(PlayerId);
            if (Player == null) return RedirectToPage("/JoinSession");

            var session = _sessionService.GetSessionById(Player.SessionId);
            if (session == null) return RedirectToPage("/JoinSession");

            Round = _gameEngine.GetRoundConfig(session.CurrentRound);
            if (Round == null) return RedirectToPage("/Results", new { playerId = PlayerId });

            //Show waiting message if other players have not completed yet
            WaitingMessage = TempData["WaitingMessage"] as string;

            return Page();
        }

        public IActionResult OnPostSelectChoice(string choiceDescription, decimal? amount)
        {
            Player = _playerService.GetPlayer(PlayerId);
            if (Player == null) return RedirectToPage("/JoinSession");

            var session = _sessionService.GetSessionById(Player.SessionId);
            if (session == null) return RedirectToPage("/JoinSession");

            Round = _gameEngine.GetRoundConfig(session.CurrentRound);
            if (Round == null) return RedirectToPage("/Results", new { playerId = PlayerId });

            // Determine cost/totalPrice based on whether round requires numeric input
            decimal monthlyCost = Round.RequiresAmount ? (amount ?? 0) : 0;
            decimal? totalPrice = Round.RequiresAmount ? amount : null;

            // Record the player's choice
            _gameEngine.RecordChoice(
                playerId: PlayerId,
                roundNumber: Round.RoundNumber,
                roundType: Round.RoundType,
                choiceDescription: choiceDescription,
                monthlyCost: monthlyCost,
                totalPrice: totalPrice
            );

            // Check if all players are done with this round
            bool allReady = _gameEngine.AreAllPlayersReady(session.Id, Round.RoundNumber);

            if (allReady)
            {
                // Advance session to next round
                _gameEngine.AdvanceRound(session.Id);

                // Redirect to next round (generic Round page)
                var nextRound = _gameEngine.GetRoundPage(session.CurrentRound);
                return RedirectToPage(nextRound, new { playerId = PlayerId });
            }

            // If other players are not ready, stay on this page
            TempData["WaitingMessage"] = "Waiting for other players...";
            return Page();
        }
    }
}