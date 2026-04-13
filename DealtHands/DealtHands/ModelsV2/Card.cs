using System;
using System.Collections.Generic;

namespace DealtHands.ModelsV2;

public partial class Card
{
    public int CardId { get; set; }

    public string RoundType { get; set; } = null!;

    public string CardType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? MonthlyAmount { get; set; }

    public string? FieldData { get; set; }

    public bool IsActive { get; set; }

    // Difficulty filter: 0 = Any, 1 = Easy+, 2 = Medium+, 3 = Hard only
    public byte DifficultyLevel { get; set; }

    // Requires* columns: game changer filtering
    // NULL = don't care, true = player must have flag, false = player must not have flag
    public bool? RequiresStudentLoans { get; set; }
    public bool? RequiresCar { get; set; }
    public bool? RequiresCarLoan { get; set; }
    public bool? RequiresOwnsHome { get; set; }
    public bool? RequiresApartment { get; set; }
    public bool? RequiresRoommate { get; set; }
    public bool? RequiresMarried { get; set; }
    public bool? RequiresChildren { get; set; }

    // Sets* columns: what player flags this RoundCard sets on submission
    // NULL = does not affect this flag
    public bool? SetsStudentLoans { get; set; }
    public bool? SetsCar { get; set; }
    public bool? SetsCarLoan { get; set; }
    public bool? SetsOwnsHome { get; set; }
    public bool? SetsApartment { get; set; }
    public bool? SetsRoommate { get; set; }
    public bool? SetsMarried { get; set; }
    public bool? SetsChildren { get; set; }

    public virtual ICollection<Ugc> Ugcs { get; set; } = new List<Ugc>();

    // Navigation property for link table (not used yet, but prevents EF errors)
    public virtual ICollection<CardLifeSituation> CardLifeSituations { get; set; } = new List<CardLifeSituation>();
}

