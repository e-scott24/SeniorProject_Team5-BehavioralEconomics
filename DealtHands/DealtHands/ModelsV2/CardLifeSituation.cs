using System;
using System.Collections.Generic;

namespace DealtHands.ModelsV2;

/*
 * NOT BEING USED CURRENTLY
 * This will be used when the DB is 
 * switched over to using link tables
*/
public partial class CardLifeSituation
{
    public int CardLifeSituationId { get; set; }

    public int CardId { get; set; }

    public int LifeSituationId { get; set; }

    public string RelationType { get; set; } = null!; // "Requires" or "Sets"

    public virtual Card Card { get; set; } = null!;

    public virtual LifeSituation LifeSituation { get; set; } = null!;
}