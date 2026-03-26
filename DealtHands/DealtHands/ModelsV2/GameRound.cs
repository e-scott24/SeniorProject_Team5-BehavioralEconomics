using System;
using System.Collections.Generic;

namespace DealtHands.ModelsV2;

public partial class GameRound
{
    public long GameRoundId { get; set; }

    public long GameSessionId { get; set; }

    public byte RoundNumber { get; set; }

    public string RoundType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? OpenedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual GameSession GameSession { get; set; } = null!;

    public virtual ICollection<Ugc> Ugcs { get; set; } = new List<Ugc>();
}
