namespace DealtHands.Models
{
    public class PlayerChoice
    {
        public int Id { get; set; }

        // Which player made this choice?
        public int PlayerId { get; set; }
        public Player Player { get; set; }

        // Which round? (1-5)
        public int RoundNumber { get; set; }

        // What type: "Career", "Loan", "Transportation", "Housing", "Family"
        public string RoundType { get; set; }

        // What did they choose?
        // e.g., "Software Developer", "2018 Honda Civic", "2BR Apartment"
        public string ChoiceDescription { get; set; }

        // How much does it cost monthly?
        public decimal MonthlyCost { get; set; }

        // If it's a purchase, what's the total price?
        public decimal? TotalPrice { get; set; }

        // Was this randomly assigned or chosen by student?
        public bool WasRandomlyAssigned { get; set; }

        // When was this choice made?
        public DateTime ChosenAt { get; set; }
    }
}