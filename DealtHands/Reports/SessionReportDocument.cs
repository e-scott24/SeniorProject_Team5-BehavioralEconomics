using DealtHands.ModelsV2;
using DealtHands.Pages;
using DealtHands.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DealtHands.Reports
{
    public class SessionReportDocument : IDocument
    {
        private readonly GameSession _session;
        private readonly List<LeaderboardEntry> _leaderboard;
        private readonly List<RoundSummary> _roundSummaries;
        private readonly List<PlayerStateInfo> _playerStates;

        public SessionReportDocument(GameSession session, List<LeaderboardEntry> leaderboard,
            List<RoundSummary> roundSummaries, List<PlayerStateInfo> playerStates)
        {
            _session = session;
            _leaderboard = leaderboard;
            _roundSummaries = roundSummaries;
            _playerStates = playerStates;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Column(col =>
                {
                    col.Item()
                        .Text($"Session Report: {_session.Name}")
                        .FontSize(18).Bold();

                    col.Item().PaddingTop(2)
                        .Text($"Join Code: {_session.JoinCode}  ·  Mode: {_session.Game?.Mode}  ·  Difficulty: {_session.Difficulty}  ·  Players: {_leaderboard.Count}")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);

                    col.Item().PaddingTop(1)
                        .Text($"Started: {_session.StartedAt?.ToString("MM/dd/yyyy hh:mm tt")}  ·  Completed: {_session.EndedAt?.ToString("MM/dd/yyyy hh:mm tt")}")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);

                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().Column(col =>
                {
                    // ── Final Leaderboard ────────────────────────────────────────────
                    col.Item().PaddingTop(12)
                        .Text("Final Leaderboard")
                        .FontSize(13).Bold();

                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(36);   // Rank
                            c.RelativeColumn(3);    // Player
                            c.RelativeColumn(2);    // Score
                            c.ConstantColumn(52);   // Choices
                            c.RelativeColumn(2);    // Health
                        });

                        table.Header(header =>
                        {
                            foreach (var label in new[] { "Rank", "Player", "Final Score", "Choices", "Financial Health" })
                            {
                                header.Cell()
                                    .Background(Colors.Blue.Darken1)
                                    .Padding(5)
                                    .Text(label)
                                    .FontColor(Colors.White)
                                    .FontSize(9).Bold();
                            }
                        });

                        int rank = 1;
                        foreach (var entry in _leaderboard)
                        {
                            var state = _playerStates.FirstOrDefault(s => s.UserId == entry.UserId);
                            var health = state?.FinancialHealth ?? "Unknown";
                            bool positive = entry.CurrentScore >= 0;
                            var bg = rank % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                            table.Cell().Background(bg).Padding(4).Text($"#{rank}").FontSize(9);
                            table.Cell().Background(bg).Padding(4).Text(entry.Username).FontSize(9);
                            table.Cell().Background(bg).Padding(4)
                                .Text($"${entry.CurrentScore:N2}")
                                .FontColor(positive ? Colors.Green.Darken2 : Colors.Red.Medium)
                                .FontSize(9);
                            table.Cell().Background(bg).Padding(4).Text(entry.CardsSubmitted.ToString()).FontSize(9);
                            table.Cell().Background(bg).Padding(4).Text(health).FontSize(9);
                            rank++;
                        }
                    });

                    // ── Round-by-Round Summary ───────────────────────────────────────
                    foreach (var round in _roundSummaries.OrderBy(r => r.RoundNumber))
                    {
                        col.Item().PaddingTop(18)
                            .Text($"Round {round.RoundNumber}: {round.RoundType}")
                            .FontSize(12).Bold();

                        col.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                foreach (var label in new[] { "Player", "Card Chosen", "Amount", "Running Total" })
                                {
                                    header.Cell()
                                        .Background(Colors.Grey.Darken1)
                                        .Padding(5)
                                        .Text(label)
                                        .FontColor(Colors.White)
                                        .FontSize(9).Bold();
                                }
                            });

                            bool alt = false;
                            foreach (var result in round.Results)
                            {
                                var bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                                bool amtPos = (result.SubmittedAmount ?? 0) >= 0;
                                bool totalPos = (result.RunningTotal ?? 0) >= 0;

                                table.Cell().Background(bg).Padding(4).Text(result.Username).FontSize(9);
                                table.Cell().Background(bg).Padding(4).Text(result.CardTitle ?? "").FontSize(9);
                                table.Cell().Background(bg).Padding(4)
                                    .Text($"{(amtPos ? "+" : "")}${result.SubmittedAmount:N2}")
                                    .FontColor(amtPos ? Colors.Green.Darken2 : Colors.Red.Medium)
                                    .FontSize(9);
                                table.Cell().Background(bg).Padding(4)
                                    .Text($"${result.RunningTotal:N2}")
                                    .FontColor(totalPos ? Colors.Green.Darken2 : Colors.Red.Medium)
                                    .FontSize(9);
                                alt = !alt;
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }
    }
}
