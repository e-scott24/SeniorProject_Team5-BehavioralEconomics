using System;
using System.Collections.Generic;

namespace DealtHands.ModelsV2;

public partial class Ugc
{
    public long Ugcid { get; set; }

    public long UserId { get; set; }

    public int CardId { get; set; }

    public long GameRoundId { get; set; }

    public long GameSessionId { get; set; }

    public decimal? SubmittedAmount { get; set; }

    public decimal? RunningTotal { get; set; }

    public DateTime AssignedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual Card Card { get; set; } = null!;

    public virtual GameRound GameRound { get; set; } = null!;

    public virtual GameSession GameSession { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
