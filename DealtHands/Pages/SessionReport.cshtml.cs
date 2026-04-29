using DealtHands.ModelsV2;
using DealtHands.Reports;
using DealtHands.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPDF.Fluent;

namespace DealtHands.Pages
{
    public class SessionReportModel : PageModel
    {
        private readonly GameSessionService _gameSessionService;
        private readonly IAuthenticationService _authService;

        public SessionReportModel(GameSessionService gameSessionService, IAuthenticationService authService)
        {
            _gameSessionService = gameSessionService;
            _authService = authService;
        }

        public GameSession? Session { get; set; }
        public List<LeaderboardEntry> Leaderboard { get; set; } = new List<LeaderboardEntry>();
        public List<RoundSummary> RoundSummaries { get; set; } = new List<RoundSummary>();
        public List<PlayerStateInfo> PlayerStates { get; set; } = new List<PlayerStateInfo>();

        public async Task<IActionResult> OnGetAsync(long sessionId)
        {
            // Must be an educator
            if (!_authService.IsEducator)
                return RedirectToPage("/Login");

            if (!_authService.UserId.HasValue)
                return RedirectToPage("/Login");

            Session = await _gameSessionService.GetSessionByIdAsync(sessionId);
            if (Session == null)
                return RedirectToPage("/EducatorDashboard");

            // Verify this educator owns this session
            if (Session.HostUserId != _authService.UserId.Value)
                return RedirectToPage("/EducatorDashboard");

            // Load leaderboard
            Leaderboard = await _gameSessionService.GetLeaderboardAsync(sessionId);

            // Load player financial states
            foreach (var entry in Leaderboard)
            {
                var state = await _gameSessionService.GetPlayerFinancialStateAsync(entry.UserId, sessionId);
                PlayerStates.Add(new PlayerStateInfo
                {
                    UserId = entry.UserId,
                    FinancialHealth = state.FinancialHealth
                });
            }

            // Load round-by-round data
            var rounds = await _gameSessionService.GetSessionRoundsAsync(sessionId);

            foreach (var round in rounds)
            {
                var results = await _gameSessionService.GetRoundResultsAsync(round.GameRoundId);

                RoundSummaries.Add(new RoundSummary
                {
                    RoundNumber = round.RoundNumber,
                    RoundType = round.RoundType,
                    Results = results
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnGetDownloadPdfAsync(long sessionId)
        {
            if (!_authService.IsEducator || !_authService.UserId.HasValue)
                return RedirectToPage("/Login");

            Session = await _gameSessionService.GetSessionByIdAsync(sessionId);
            if (Session == null || Session.HostUserId != _authService.UserId.Value)
                return RedirectToPage("/EducatorDashboard");

            Leaderboard = await _gameSessionService.GetLeaderboardAsync(sessionId);
            foreach (var entry in Leaderboard)
            {
                var state = await _gameSessionService.GetPlayerFinancialStateAsync(entry.UserId, sessionId);
                PlayerStates.Add(new PlayerStateInfo { UserId = entry.UserId, FinancialHealth = state.FinancialHealth });
            }

            var rounds = await _gameSessionService.GetSessionRoundsAsync(sessionId);
            foreach (var round in rounds)
            {
                var results = await _gameSessionService.GetRoundResultsAsync(round.GameRoundId);
                RoundSummaries.Add(new RoundSummary
                {
                    RoundNumber = round.RoundNumber,
                    RoundType = round.RoundType,
                    Results = results
                });
            }

            var document = new SessionReportDocument(Session, Leaderboard, RoundSummaries, PlayerStates);
            var pdf = document.GeneratePdf();
            return File(pdf, "application/pdf", $"Session_Report_{Session.JoinCode}.pdf");
        }
    }

    // Helper class to store player financial health
    public class PlayerStateInfo
    {
        public long UserId { get; set; }
        public string FinancialHealth { get; set; } = "Unknown";
    }

    // Helper class to organize round data
    public class RoundSummary
    {
        public byte RoundNumber { get; set; }
        public string RoundType { get; set; } = string.Empty;
        public List<RoundResult> Results { get; set; } = new List<RoundResult>();
    }
}