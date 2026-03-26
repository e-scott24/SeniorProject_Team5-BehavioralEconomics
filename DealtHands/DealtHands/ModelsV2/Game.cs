using System;
using System.Collections.Generic;

namespace DealtHands.ModelsV2;

public partial class Game
{
    public long GameId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Mode { get; set; } = null!;

    public int? MaxPlayers { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
}
