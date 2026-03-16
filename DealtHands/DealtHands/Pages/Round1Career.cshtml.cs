using DealtHands.Models;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealtHands.Pages
{
    public class Round1CareerModel : PageModel
    {
        private readonly GameEngine _gameEngine;
        private readonly PlayerService _playerService;
        private readonly SessionService _sessionService;

        public Round1CareerModel(GameEngine gameEngine, PlayerService playerService, SessionService sessionService)
        {
            _gameEngine = gameEngine;
            _playerService = playerService;
            _sessionService = sessionService;
        }

        public Player Player { get; set; }

        /*
        public void OnGet(int playerId)
        {
            Player = _playerService.GetPlayer(playerId);
        }
        */


        public IActionResult OnGet(int playerId)
        {
            Player = _playerService.GetPlayer(playerId);

            if (Player == null)
            {
                return RedirectToPage("/JoinSession");
                //return NotFound("Player not found");
            }

            return Page();

        }

        public IActionResult OnPostSelectCareer(int playerId, string careerName, decimal salary)
        {

            var player = _playerService.GetPlayer(playerId);

            if (player == null)
            {
                return RedirectToPage("/JoinSession");
                //return NotFound("Player not found");
            }


            var session = _sessionService.GetSessionById(player.SessionId);

            if (session == null)
            {
                return RedirectToPage("/JoinSession");
                //return NotFound("Session not found");
            }

            // Record the choice
            _gameEngine.RecordChoice(
                playerId: playerId,
                roundNumber: 1,
                roundType: "Career",
                choiceDescription: careerName,
                monthlyCost: 0,
                totalPrice: salary
            );

            // Check if all players are ready to move to next round
            bool allReady = _gameEngine.AreAllPlayersReady(session.Id, roundNumber: 1);

            if (allReady)
            {
                //Advance to next round
                _gameEngine.AdvanceRound(session.Id);
                var nextRound = _gameEngine.GetRoundPage(session.CurrentRound);
                return RedirectToPage(nextRound, new { playerId = playerId });
            }

            /*
            // Move to next round
            return RedirectToPage("/Round2Loans", new { playerId = playerId });
            */

            // If not all players are ready, stay on the same page and show waiting message
            TempData["WaitingMessage"] = "Waiting for other players to select their careers...";
            return Page();
            //return RedirectToPage("/Lobby", new { sessionCode = session.SessionCode, playerId = playerId });
        }
    }
}