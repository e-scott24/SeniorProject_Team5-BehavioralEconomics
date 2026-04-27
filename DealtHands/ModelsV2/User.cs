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

    // Player state flags — set automatically when a student submits a RoundCard choice
    public bool HasStudentLoans { get; set; }
    public bool HasCar { get; set; }
    public bool HasCarLoan { get; set; }
    public bool OwnsHome { get; set; }
    public bool HasApartment { get; set; }
    public bool HasRoommate { get; set; }
    public bool IsMarried { get; set; }
    public bool HasChildren { get; set; }

    public bool HasJob { get; set; }

    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();

    public virtual ICollection<Ugc> Ugcs { get; set; } = new List<Ugc>();
}
