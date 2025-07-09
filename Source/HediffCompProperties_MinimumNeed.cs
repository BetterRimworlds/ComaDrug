namespace BetterRimworlds.ComaDrug;

using Verse;
using System.Collections.Generic;

public class HediffCompProperties_MinimumNeed : HediffCompProperties
{
    // This will hold the list of <needs> from the XML.
    public List<NeedLevel> needs;

    public HediffCompProperties_MinimumNeed()
    {
        this.compClass = typeof(HediffComp_MinimumNeed);
    }
}
