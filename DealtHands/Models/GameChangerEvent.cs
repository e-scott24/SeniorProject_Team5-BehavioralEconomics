/*  Name: Jason Black
    Date: 3/20/2026
    Last Update: 3/21/2026

    Raw data definition of a Game Changer event.
    Need to replace the static list in GameChangerService with a database query later.

    The correct file paths are the following:
    ...\DealtHands\DealtHands\Models\GameChangerEvent.cs
    ...\DealtHands\DealtHands\Models\Cards\GameChangerCard.cs
    ...\DealtHands\DealtHands\Services\GameChangerService.cs
*/

namespace DealtHands.Models
{
    public enum CardType
    {
        Positive,
        Negative
    }

    public enum CardKind
    {
        Salary,
        Balance,
        JobLoss,
        Debt,
        Recurring,
        Combo
    }

    public class GameChangerEvent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPositive { get; set; }
        public int TriggersInRound { get; set; }
        public string DifficultyLevel { get; set; }  // "Easy", "Medium", "Hard", "All"
        public string Category { get; set; }
        public CardType CardType { get; set; }        // Positive or Negative 
        public CardKind CardKind { get; set; }
        public decimal SalaryChange { get; set; }
        public decimal BalanceChange { get; set; }
        public bool IsJobLoss { get; set; }
        public decimal DebtChange { get; set; }
        public string EffectLabel { get; set; }
        public string EffectAmount { get; set; }
        public string EffectNote { get; set; }
        public string BadgeText { get; set; }
        public string SecondaryLabel { get; set; }
        public string SecondaryAmountDisplay { get; set; }
    }
}
