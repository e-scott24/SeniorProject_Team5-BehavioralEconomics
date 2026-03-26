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

    public virtual ICollection<Ugc> Ugcs { get; set; } = new List<Ugc>();
}
