/*  Name: Jason Black
    Date: 3/20/2026
    Last Update: 3/21/2026

    Directly adjusts TotalDebt without touching balance.
    Negative DebtChange = debt reduced (good).
    Positive DebtChange = debt added (bad).
*/

namespace DealtHands.Models.Cards
{
    public class DebtCard : GameChangerCard
    {
        // DebtCard — directly adjusts TotalDebt 
        public decimal DebtChange { get; set; }

        public override void Apply(Player player)
        {
            player.TotalDebt = Math.Max(0, player.TotalDebt + DebtChange);
        }
    }
}
