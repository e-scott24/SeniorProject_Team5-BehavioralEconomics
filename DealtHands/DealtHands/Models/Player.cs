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


        // Player Code for joining the game after it is paused
        // and then resumed to keep player's data/progress
        public string PlayerCode { get; set; } // 4-digit unique code per player

        // When did they join?
        public DateTime JoinedAt { get; set; }

        // Are they still connected/active?
        public bool IsActive { get; set; }

        // ===== FINANCIAL STATE =====
        // Current yearly salary
        public decimal Salary { get; set; }
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