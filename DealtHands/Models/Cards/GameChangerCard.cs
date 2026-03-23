/*  Name: Jason Black
    Date: 3/20/2026
    Last Update: 3/21/2026

    Abstract base class for all Game Changer card types.
    Holds all display fields used by _GameChangerCard.cshtml.
    Each subclass implements Apply() with its own financial logic.
    Built by GameChangerService.MapToCard() from a GameChangerEvent.
    Rendered by _GameChangerCard.cshtml.

    The correct file paths are the following:
    ...\DealtHands\DealtHands\Services\GameChangerService.cs
    ...\DealtHands\DealtHands\Services\GameEngine.cs
    ...\DealtHands\DealtHands\Models\GameChangerEvent.cs
    ...\DealtHands\DealtHands\Models\Cards\GameChangerCard.cs
    ...\DealtHands\DealtHands\Models\Cards\SalaryCard.cs
    ...\DealtHands\DealtHands\Models\Cards\BalanceCard.cs
    ...\DealtHands\DealtHands\Models\Cards\JobLossCard.cs
    ...\DealtHands\DealtHands\Models\Cards\DebtCard.cs
    ...\DealtHands\DealtHands\Models\Cards\RecurringCard.cs
    ...\DealtHands\DealtHands\Models\Cards\ComboCard.cs
    ...\DealtHands\DealtHands\Pages\Shared\GameChangerOverlayModel.cs
    ...\DealtHands\DealtHands\Pages\Shared\Cards\_GameChangerCard.cshtml
    ...\DealtHands\DealtHands\Pages\Shared\Cards\_GameChangerOverlay.cshtml
*/

namespace DealtHands.Models.Cards
{
    public abstract class GameChangerCard
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPositive { get; set; }        // true = green, false = red

        public string EffectLabel { get; set; }
        public string EffectAmount { get; set; }
        public string EffectNote { get; set; }
        public string BadgeText { get; set; }
        public string SecondaryLabel { get; set; }
        public string SecondaryAmount { get; set; }

        // SalaryCard/RecurringCard — permanent monthly income change
        public abstract void Apply(Player player);

        protected static void ApplyWaterfall(Player player, decimal amount)
        {
            if (player.Balance >= amount)
            {
                player.Balance -= amount;
            }
            else
            {
                amount -= player.Balance;
                player.Balance = 0;

                if (player.Savings >= amount)
                {
                    player.Savings -= amount;
                }
                else
                {
                    amount -= player.Savings;
                    player.Savings = 0;
                    player.TotalDebt += amount;
                }
            }
        }
    }
}
