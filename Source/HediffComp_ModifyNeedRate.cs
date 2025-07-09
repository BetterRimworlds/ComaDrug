/*
 * This file is part of Coma Drug, a Better Rimworlds Project.
 *
 * Copyright © 2025 Theodore R. Smith
 * Author: Theodore R. Smith <hopeseekr@gmail.com>
 *   GPG Fingerprint: D8EA 6E4D 5952 159D 7759  2BB4 EEB6 CE72 F441 EC41
 *   https://github.com/BetterRimworlds/ComaDrug
 *   https://steamcommunity.com/sharedfiles/filedetails/?id=3442966730
 *
 * This file is licensed under the Creative Commons CC BY-ND v4.0 License.
 */

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
