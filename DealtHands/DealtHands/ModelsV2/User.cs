using System;
using System.Collections.Generic;

namespace DealtHands.ModelsV2;

public partial class User
{
    public long UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    // True for educator accounts, false for student accounts
    public bool IsEducator { get; set; }

    // For password resets
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpires { get; set; }

    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();

    public virtual ICollection<Ugc> Ugcs { get; set; } = new List<Ugc>();
}