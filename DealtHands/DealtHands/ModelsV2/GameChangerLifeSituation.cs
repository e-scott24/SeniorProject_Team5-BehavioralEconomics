using System;
using System.Collections.Generic;

namespace DealtHands.ModelsV2;

/*
 * NOT BEING USED CURRENTLY
 * This will be used when the DB is 
 * switched over to using link tables
*/
public partial class GameChangerLifeSituation
{
    public int GameChangerLifeSituationId { get; set; }

    public int GameChangerId { get; set; }

    public int LifeSituationId { get; set; }

    public string RelationType { get; set; } = null!; // "Requires" or "Sets"

    public virtual GameChanger GameChanger { get; set; } = null!;

    public virtual LifeSituation LifeSituation { get; set; } = null!;
}