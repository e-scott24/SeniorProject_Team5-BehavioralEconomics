/*  Name: Jason Black
    Date: 3/20/2026
    Last Update: 3/21/2026

    Model passed to _GameChangerOverlay.cshtml.
    Every round page creates one of these to pass cards, playerId,
    and the next page URL into the overlay partial.

    The correct file paths are the following:
    ...\DealtHands\DealtHands\Services\GameChangerService.cs
    ...\DealtHands\DealtHands\Services\GameEngine.cs
    ...\DealtHands\DealtHands\Models\GameChangerEvent.cs
    ...\DealtHands\DealtHands\Models\Cards\GameChangerCard.cs
    ...\DealtHands\DealtHands\Pages\Shared\GameChangerOverlayModel.cs
    ...\DealtHands\DealtHands\Pages\Shared\Cards\_GameChangerCard.cshtml
    ...\DealtHands\DealtHands\Pages\Shared\Cards\_GameChangerOverlay.cshtml
*/

using DealtHands.Models.Cards;

namespace DealtHands.Pages.Shared
{
    // Model passed to _GameChangerOverlay.cshtml.
    // Every round page creates one of these and passes it to the partial.
    public class GameChangerOverlayModel
    {
        // The shuffled cards for this round
        public List<GameChangerCard> Cards { get; set; } = new();

        // The current player — used to POST ApplyCard and build the redirect URL
        public int PlayerId { get; set; }

        // Where to send the player after all cards are done
        public string NextUrl { get; set; } = string.Empty;
    }
}
