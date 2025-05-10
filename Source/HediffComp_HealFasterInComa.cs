// ==== Source/HediffComp_HealFasterInComa.cs ====
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

public class HediffCompProperties_HealFasterInComa : HediffCompProperties
{
    /// <summary>Multiplier for natural healing speed. 2.0 = 2x healing rate.</summary>
    public float healFactor = 10.0f;

    /// <summary>Interval in ticks between healing applications. Default: 60 (1 second).</summary>
    public int intervalTicks = 60;

    public HediffCompProperties_HealFasterInComa()
    {
        this.compClass = typeof(HediffComp_HealFasterInComa);
    }
}

public class HediffComp_HealFasterInComa : HediffComp
{
    private HediffCompProperties_HealFasterInComa Props =>
        (HediffCompProperties_HealFasterInComa)this.props;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);

        if (
            !this.Pawn.IsHashIntervalTick(Props.intervalTicks) ||
            this.Pawn.health?.hediffSet?.hediffs == null
        )
        {
            return;
        }

        List<Hediff> hediffs = this.Pawn.health.hediffSet.hediffs;

        // Vanilla heals ~0.01 severity per 2500 ticks.
        // We only add the *extra* healing on top of vanilla.
        // healFactor=2.0 means 1x extra (vanilla already provides 1x).
        float extraHealAmount =
            0.01f
            * ((float)Props.intervalTicks / 2500f)
            * (Props.healFactor - 1f);

        if (extraHealAmount <= 0f) return;

        for (int i = hediffs.Count - 1; i >= 0; i--)
        {
            if (i >= hediffs.Count) continue;

            if (
                hediffs[i] is Hediff_Injury injury &&
                injury.CanHealNaturally()
            )
            {
                injury.Severity -= extraHealAmount;
                if (injury.Severity <= 0f)
                {
                    this.Pawn.health.RemoveHediff(injury);
                }
            }
        }
    }
}