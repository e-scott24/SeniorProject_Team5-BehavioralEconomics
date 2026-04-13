using System;
using System.Collections.Generic;

namespace DealtHands.ModelsV2;

/*
 * NOT BEING USED CURRENTLY
 * This will be used when the DB is 
 * switched over to using link tables
*/ 

public partial class LifeSituation
{
    public int LifeSituationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<CardLifeSituation> CardLifeSituations { get; set; } = new List<CardLifeSituation>();

    public virtual ICollection<GameChangerLifeSituation> GameChangerLifeSituations { get; set; } = new List<GameChangerLifeSituation>();
}