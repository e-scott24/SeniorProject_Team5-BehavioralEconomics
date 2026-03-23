/*  Name: Jason Black
    Date: 3/20/2026
    Last Update: 3/21/2026

    Salary change AND balance hit simultaneously.
    e.g. promotion with signing bonus, or pay cut with penalty payment.
*/

namespace DealtHands.Models.Cards
{
    public class ComboCard : GameChangerCard
    {
        public decimal SalaryChange { get; set; }
        public decimal BalanceChange { get; set; }

        public override void Apply(Player player)
        {
            player.MonthlyIncome = Math.Max(0, player.MonthlyIncome + SalaryChange);
            player.Salary = player.MonthlyIncome * 12;

            if (BalanceChange > 0)
            {
                player.Balance += BalanceChange;
            }
            else if (BalanceChange < 0)
            {
                ApplyWaterfall(player, Math.Abs(BalanceChange));
            }
        }
    }
}
