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
        // Current monthly income
        public decimal MonthlyIncome { get; set; }

        // Current monthly expenses (sum of all)
        public decimal MonthlyExpenses { get; set; }

        // Total debt accumulated
        public decimal TotalDebt { get; set; }

        // Emergency savings
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