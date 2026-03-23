/*  Name: Jason Black
    Date: 3/20/2026
    Last Update: 3/21/2026

    Permanent monthly expense — semantically an expense, not income.
    Uses the same MonthlyIncome field as SalaryCard but represents
    a recurring bill (insurance, subscription, etc.) not a salary change.
*/

namespace DealtHands.Models.Cards
{
    public class RecurringCard : GameChangerCard
    {
        public decimal MonthlyChange { get; set; }

        public override void Apply(Player player)
        {
            player.MonthlyIncome = Math.Max(0, player.MonthlyIncome + MonthlyChange);
            player.Salary = player.MonthlyIncome * 12;
        }
    }
}
