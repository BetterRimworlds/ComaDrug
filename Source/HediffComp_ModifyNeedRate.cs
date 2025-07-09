namespace BetterRimworlds.ComaDrug;

using System.Collections.Generic;
using RimWorld;
using Verse;

public class NeedRateModifier
{
    public NeedDef needDef;
    public float   rateFactor = 1f;
}

public class HediffCompProperties_ModifyNeedRate : HediffCompProperties
{
    public List<NeedRateModifier> needs;
    public HediffCompProperties_ModifyNeedRate()
    {
        this.compClass = typeof(HediffComp_ModifyNeedRate);
    }
}

public class HediffComp_ModifyNeedRate : HediffComp
{
    private HediffCompProperties_ModifyNeedRate Props => (HediffCompProperties_ModifyNeedRate)this.props;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);

        const int checkInterval = 30;
        if (this.Pawn.IsHashIntervalTick(checkInterval))
        {
            if (this.Pawn?.needs == null) return;

            foreach (var mod in Props.needs)
            {
                var need = this.Pawn.needs.TryGetNeed(mod.needDef);
                if (need == null) continue;

                float fullDropPerTick = need.def.fallPerDay / GenDate.TicksPerDay;
                float amountToRestorePerTick = fullDropPerTick * (1f - mod.rateFactor);
                need.CurLevel += amountToRestorePerTick * checkInterval;
            }
        }
    }
}
