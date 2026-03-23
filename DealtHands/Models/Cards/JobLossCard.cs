/*  Name: Jason Black
    Date: 3/20/2026
    Last Update: 3/21/2026

    Zeros monthly income + adds a one-time payout to balance.
    Income cannot be restored without a future re-employment card.
*/

namespace DealtHands.Models.Cards
{
    public class JobLossCard : GameChangerCard
    {
        // JobLossCard tells GameEngine to remove income
        public decimal Severance { get; set; }

        public override void Apply(Player player)
        {
            player.MonthlyIncome = 0;
            player.Salary = 0;
            player.Balance += Severance;
        }
    }
}
