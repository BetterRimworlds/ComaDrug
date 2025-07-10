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

/// <summary>
/// The logic component that enforces minimum need levels while the hediff is active.
/// </summary>
public class HediffComp_MinimumNeed : HediffComp
{
    // A private property to easily access our typed properties.
    private HediffCompProperties_MinimumNeed Props => (HediffCompProperties_MinimumNeed)this.props;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);

        // If the pawn is dead or despawned, do nothing.
        if (this.Pawn == null || this.Pawn.needs == null) return;

        // We only need to check periodically, not every single tick.
        // Checking every 30 ticks is more than enough and better for performance.
        if (this.Pawn.IsHashIntervalTick(60))
        {
            // Loop through every need defined in our XML properties.
            foreach (var needEntry in Props.needs)
            {
                // Find the pawn's actual need that matches the one from our list.
                Need need = this.Pawn.needs.TryGetNeed(needEntry.needDef);

                // CRITICAL: Always check if the need is null. A pawn who is not
                // addicted to Luciferium will not have the 'Luciferium' need.
                if (need != null)
                {
                    // If the need's current level is below our defined minimum, top it up.
                    if (need.CurLevel < needEntry.minLevel)
                    {
                        need.CurLevel = needEntry.minLevel;
                    }
                }
            }
        }
    }
}

public class HediffCompProperties_MinimumNeed : HediffCompProperties
{
    // This will hold the list of <needs> from the XML.
    public List<NeedLevel> needs;

    public HediffCompProperties_MinimumNeed()
    {
        this.compClass = typeof(HediffComp_MinimumNeed);
    }
}
