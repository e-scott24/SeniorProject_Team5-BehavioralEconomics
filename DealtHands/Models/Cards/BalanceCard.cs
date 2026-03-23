/*  Name: Jason Black
    Date: 3/20/2026
    Last Update: 3/21/2026

    One-time balance credit or debit.
    Negative values use the waterfall: Balance → Savings → TotalDebt.
*/

namespace DealtHands.Models.Cards
{
    public class BalanceCard : GameChangerCard
    {
        // BalanceCard/ComboCard/JobLossCard severance
        public decimal BalanceChange { get; set; }

        public override void Apply(Player player)
        {
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
