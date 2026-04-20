using System;
using System.Collections.Generic;

namespace DealtHands.ModelsV2;

public partial class GameChanger
{
    public int GameChangerId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string RoundType { get; set; } = null!;

    public byte DifficultyLevel { get; set; }

    public decimal? MonthlyAmount { get; set; }

    public bool IsActive { get; set; }

    // Filtering flags — NULL = don't care, true = must have, false = must not have
    public bool? RequiresApartment { get; set; }
    public bool? RequiresCar { get; set; }
    public bool? RequiresCarLoan { get; set; }
    public bool? RequiresOwnsHome { get; set; }
    public bool? RequiresStudentLoans { get; set; }
    public bool? RequiresRoommate { get; set; }
    public bool? RequiresMarried { get; set; }
    public bool? RequiresChildren { get; set; }
    public bool? RequiresJob { get; set; }

    // Financial effect columns
    public decimal? IncomeEffect { get; set; }
    public decimal? IncomeEffectPercent { get; set; }
    public decimal? ExpenseEffect { get; set; }

    // Relative selection weight used by the weighted-random picker in GameSessionService.
    // Higher weight = more likely to fire when this card survives the filter.
    // Common events should be 8–10, dramatic events (Laid Off, Terminated) should be 1–2.
    // NULL defaults to 5 at pick time.
    public int? Weight { get; set; }

    // Player state mutations applied when this card is drawn
    public bool? SetsApartment { get; set; }
    public bool? SetsCar { get; set; }
    public bool? SetsCarLoan { get; set; }
    public bool? SetsOwnsHome { get; set; }
    public bool? SetsStudentLoans { get; set; }
    public bool? SetsRoommate { get; set; }
    public bool? SetsMarried { get; set; }
    public bool? SetsChildren { get; set; }
    public bool? SetsJob { get; set; }

    public virtual ICollection<Ugc> Ugcs { get; set; } = new List<Ugc>();
}
