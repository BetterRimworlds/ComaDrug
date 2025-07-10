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

namespace BetterRimworlds.ComaDrug; // C# 10 file-scoped namespace

using RimWorld;
using Verse;

/// <summary>
/// This component actively checks if a pawn has a specific "trigger" hediff.
/// If the trigger hediff is found, this component immediately removes its parent hediff.
/// </summary>
public class HediffComp_WakeOnHediff : HediffComp
{
    // A private property to easily access our typed properties.
    private HediffCompProperties_WakeOnHediff Props => (HediffCompProperties_WakeOnHediff)this.props;

    // CompPostTick runs continuously while the hediff is active.
    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);

        // Defensive checks: ensure the pawn and its health tracker exist,
        // and that a wakeHediff was actually specified in the XML.
        // If the pawn is dead or despawned, do nothing.
        if (this.Pawn == null || this.Pawn.health?.hediffSet == null || Props.wakeHediff == null)
            return;

        // For performance, we only check once per second (60 ticks), not every single tick.
        if (this.Pawn.IsHashIntervalTick(60))
        {
            // Check if the pawn's list of hediffs contains the one we're looking for.
            if (this.Pawn.health.hediffSet.HasHediff(Props.wakeHediff))
            {
                // If it does, tell the pawn's health tracker to remove our parent hediff (the coma).
                // This instantly ends the coma and all its effects.
                this.Pawn.health.RemoveHediff(this.parent);
            }
        }
    }
}

/// <summary>
/// Properties for the WakeOnHediff component. This tells the component
/// which hediff to look for as the "wake-up" trigger.
/// </summary>
public class HediffCompProperties_WakeOnHediff : HediffCompProperties
{
    // This will be set in the XML to the hediff that should trigger the removal.
    // For this case, it will be WakeUpHigh.
    public HediffDef wakeHediff;

    public HediffCompProperties_WakeOnHediff()
    {
        this.compClass = typeof(HediffComp_WakeOnHediff);
    }
}
