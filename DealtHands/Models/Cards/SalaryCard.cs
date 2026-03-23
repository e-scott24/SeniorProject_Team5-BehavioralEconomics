/*  Name: Jason Black
    Date: 3/20/2026
    Last Update: 3/21/2026

    Permanent monthly income change (positive or negative).
    Income floors at 0 — never goes negative.
*/

namespace DealtHands.Models.Cards
{
    public class SalaryCard : GameChangerCard
    {
        public decimal SalaryChange { get; set; }

        public override void Apply(Player player)
        {
            player.MonthlyIncome = Math.Max(0, player.MonthlyIncome + SalaryChange);
            player.Salary = player.MonthlyIncome * 12;
        }
    }
}
