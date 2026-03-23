namespace DealtHands.Models
{
    public class Player
    {
        public int Id { get; set; }

        // Student's display name
        public string Name { get; set; }

        // Which session are they in?
        public int SessionId { get; set; }
        public Session Session { get; set; }

        // When did they join?
        public DateTime JoinedAt { get; set; }

        // Are they still connected/active?
        public bool IsActive { get; set; }

        // ===== FINANCIAL STATE =====

        // Current yearly salary
        public decimal Salary { get; set; }

        // Current monthly income
        public decimal MonthlyIncome { get; set; }

        // Current monthly expenses (sum of all choices)
        public decimal MonthlyExpenses { get; set; }

        // Main wallet — lump sums (bonuses, sick days, severance) hit here.
        // If this hits 0, debt accumulates. Savings are the last resort before debt.
        public decimal Balance { get; set; }

        // Total debt accumulated (Balance hit 0 AND Savings hit 0)
        public decimal TotalDebt { get; set; }

        // Emergency savings — player deliberately moves money here.
        // Drawn from only when Balance is exhausted, before going into debt.
        public decimal Savings { get; set; }

        // "Healthy", "Struggling", "Critical"
        public string FinancialHealth { get; set; }

        // ===== RELATIONSHIPS =====

        // All the choices this player made
        public List<PlayerChoice> Choices { get; set; } = new List<PlayerChoice>();

        // All game changers that affected this player
        public List<PlayerGameChanger> GameChangersReceived { get; set; } = new List<PlayerGameChanger>();
    }
}
