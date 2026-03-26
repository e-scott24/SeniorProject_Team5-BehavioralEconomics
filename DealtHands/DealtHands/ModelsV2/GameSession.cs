using System;
using System.Collections.Generic;

namespace DealtHands.ModelsV2;

public partial class GameSession
{
    public long GameSessionId { get; set; }

    public long GameId { get; set; }

    public long HostUserId { get; set; }

    public string JoinCode { get; set; } = null!;

    public string Status { get; set; } = null!;

    public byte CurrentRoundNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? PausedAt { get; set; }

    public DateTime? ResumedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual ICollection<GameRound> GameRounds { get; set; } = new List<GameRound>();

    public virtual User HostUser { get; set; } = null!;

    public virtual ICollection<Ugc> Ugcs { get; set; } = new List<Ugc>();
}
