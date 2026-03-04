namespace DealtHands.Models
{
    public class GameChangerEvent
    {
        public int Id { get; set; }

        // Title: "Car Transmission Failure"
        public string Title { get; set; }

        // Full description of what happened
        public string Description { get; set; }

        // Financial impact: -3500 (negative = cost, positive = gain)
        public decimal FinancialImpact { get; set; }

        // Is this positive (money gained) or negative (money lost)?
        public bool IsPositive { get; set; }

        // Which round does this happen in? (1-5)
        public int TriggersInRound { get; set; }

        // Which difficulty? "Easy", "Medium", "Hard", or "All"
        public string DifficultyLevel { get; set; }

        // Category: "Career", "Transportation", "Housing", "Family", "Health"
        public string Category { get; set; }
    }
}