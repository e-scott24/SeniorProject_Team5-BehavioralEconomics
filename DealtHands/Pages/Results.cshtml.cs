using DealtHands.ModelsV2;
using DealtHands.Reports;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPDF.Fluent;

namespace DealtHands.Pages
{
    public class ResultsModel : PageModel
    {
        private readonly GameSessionService _gameSessionService;
        private readonly IAuthenticationService _authService;

        public ResultsModel(GameSessionService gameSessionService, IAuthenticationService authService)
        {
            _gameSessionService = gameSessionService;
            _authService = authService;
        }

        public List<LeaderboardEntry> Leaderboard { get; set; } = new List<LeaderboardEntry>();
        public List<Ugc> PlayerHistory { get; set; } = new List<Ugc>();
        public PlayerFinancialState FinancialState { get; set; }
        public int PlayerRank { get; set; }
        public decimal PlayerScore { get; set; }
        public int TotalPlayers { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Hard stop — educators don't have a player GameSessionId
            if (_authService.IsEducator)
                return RedirectToPage("/EducatorDashboard");

            if (!_authService.UserId.HasValue)
                return RedirectToPage("/JoinSession");

            if (!_authService.GameSessionId.HasValue)
                return RedirectToPage("/JoinSession");

            long userId = _authService.UserId.Value;
            long gameSessionId = _authService.GameSessionId.Value;

            Leaderboard = await _gameSessionService.GetLeaderboardAsync(gameSessionId);
            PlayerHistory = await _gameSessionService.GetPlayerHistoryAsync(userId, gameSessionId);
            FinancialState = await _gameSessionService.GetPlayerFinancialStateAsync(userId, gameSessionId);

            // Compute this player's rank from the ordered leaderboard
            TotalPlayers = Leaderboard.Count;
            var myEntry = Leaderboard.FirstOrDefault(e => e.UserId == userId);
            if (myEntry != null)
            {
                PlayerRank = Leaderboard.IndexOf(myEntry) + 1;
                PlayerScore = myEntry.CurrentScore;
            }

            return Page();
        }

        public async Task<IActionResult> OnGetDownloadPdfAsync()
        {
            if (_authService.IsEducator || !_authService.UserId.HasValue || !_authService.GameSessionId.HasValue)
                return RedirectToPage("/JoinSession");

            long userId = _authService.UserId.Value;
            long gameSessionId = _authService.GameSessionId.Value;

            var leaderboard = await _gameSessionService.GetLeaderboardAsync(gameSessionId);
            var history = await _gameSessionService.GetPlayerHistoryAsync(userId, gameSessionId);
            var financialState = await _gameSessionService.GetPlayerFinancialStateAsync(userId, gameSessionId);

            int totalPlayers = leaderboard.Count;
            var myEntry = leaderboard.FirstOrDefault(e => e.UserId == userId);
            int playerRank = myEntry != null ? leaderboard.IndexOf(myEntry) + 1 : 0;
            decimal playerScore = myEntry?.CurrentScore ?? 0;

            var document = new PlayerResultsDocument(
                _authService.Username ?? "Player",
                financialState, history, playerRank, totalPlayers, playerScore);

            var pdf = document.GeneratePdf();
            return File(pdf, "application/pdf", "DealtHands_My_Results.pdf");
        }
    }
}
