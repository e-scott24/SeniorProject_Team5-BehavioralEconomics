using DealtHands.ModelsV2;
using DealtHands.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DealtHands.Reports
{
    public class PlayerResultsDocument : IDocument
    {
        private readonly string _playerName;
        private readonly PlayerFinancialState _financialState;
        private readonly List<Ugc> _history;
        private readonly int _playerRank;
        private readonly int _totalPlayers;
        private readonly decimal _playerScore;

        public PlayerResultsDocument(string playerName, PlayerFinancialState financialState,
            List<Ugc> history, int playerRank, int totalPlayers, decimal playerScore)
        {
            _playerName = playerName;
            _financialState = financialState;
            _history = history;
            _playerRank = playerRank;
            _totalPlayers = totalPlayers;
            _playerScore = playerScore;
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
                    col.Item().Text("DealtHands — Game Results Report").FontSize(18).Bold();
                    col.Item().PaddingTop(2)
                        .Text($"Player: {_playerName}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().Column(col =>
                {
                    // ── Financial Summary ────────────────────────────────────────────
                    col.Item().PaddingTop(12).Text("Financial Summary").FontSize(13).Bold();

                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        // Labels row
                        foreach (var label in new[] { "Monthly Income", "Monthly Expenses", "Monthly Available", "Financial Health" })
                        {
                            table.Cell()
                                .Background(Colors.Blue.Darken1)
                                .Padding(6)
                                .Text(label)
                                .FontColor(Colors.White)
                                .FontSize(9).Bold();
                        }

                        // Values row
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6)
                            .Text($"${_financialState.MonthlyIncome:N0}")
                            .FontColor(Colors.Green.Darken2).FontSize(11).Bold();

                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6)
                            .Text($"${_financialState.MonthlyExpenses:N0}")
                            .FontColor(Colors.Red.Medium).FontSize(11).Bold();

                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6)
                            .Text($"${_financialState.Available:N0}")
                            .FontColor(_financialState.Available >= 0 ? Colors.Green.Darken2 : Colors.Red.Medium)
                            .FontSize(11).Bold();

                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6)
                            .Text(_financialState.FinancialHealth)
                            .FontSize(11).Bold();
                    });

                    // ── Ranking ──────────────────────────────────────────────────────
                    if (_totalPlayers > 1 && _playerRank > 0)
                    {
                        col.Item().PaddingTop(16).Text("Class Ranking").FontSize(13).Bold();

                        col.Item().PaddingTop(6).Background(Colors.Grey.Lighten4)
                            .Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(12).Row(row =>
                            {
                                row.RelativeItem().Text($"#{_playerRank} of {_totalPlayers} players")
                                    .FontSize(16).Bold();
                                row.RelativeItem().AlignRight()
                                    .Text($"Final Score: ${_playerScore:N2}")
                                    .FontSize(12)
                                    .FontColor(_playerScore >= 0 ? Colors.Green.Darken2 : Colors.Red.Medium);
                            });
                    }

                    // ── Round-by-Round Breakdown ─────────────────────────────────────
                    col.Item().PaddingTop(16).Text("Round-by-Round Breakdown").FontSize(13).Bold();

                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(45);  // Round #
                            c.RelativeColumn(2);   // Type
                            c.RelativeColumn(3);   // Card
                            c.RelativeColumn(2);   // Amount
                            c.RelativeColumn(2);   // Running Total
                        });

                        table.Header(header =>
                        {
                            foreach (var label in new[] { "Round", "Type", "Card", "Amount", "Running Total" })
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
                        foreach (var ugc in _history.OrderBy(u => u.AssignedAt))
                        {
                            var bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                            bool amtPos = (ugc.SubmittedAmount ?? 0) >= 0;
                            bool totalPos = (ugc.RunningTotal ?? 0) >= 0;
                            bool isGc = ugc.GameChangerId != null;
                            var cardTitle = ugc.Card?.Title ?? ugc.GameChanger?.Title ?? "";

                            table.Cell().Background(bg).Padding(4)
                                .Text($"Round {ugc.GameRound?.RoundNumber}").FontSize(9);
                            table.Cell().Background(bg).Padding(4)
                                .Text(ugc.GameRound?.RoundType ?? "").FontSize(9);
                            table.Cell().Background(bg).Padding(4).Column(c =>
                            {
                                c.Item().Text(cardTitle).FontSize(9);
                                if (isGc)
                                    c.Item().Text("Game Changer").FontSize(7)
                                        .FontColor(Colors.Orange.Darken2).Bold();
                            });
                            table.Cell().Background(bg).Padding(4)
                                .Text($"{(amtPos ? "+" : "")}${ugc.SubmittedAmount:N2}")
                                .FontColor(amtPos ? Colors.Green.Darken2 : Colors.Red.Medium)
                                .FontSize(9);
                            table.Cell().Background(bg).Padding(4)
                                .Text($"${ugc.RunningTotal:N2}")
                                .FontColor(totalPos ? Colors.Green.Darken2 : Colors.Red.Medium)
                                .FontSize(9);
                            alt = !alt;
                        }
                    });
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
