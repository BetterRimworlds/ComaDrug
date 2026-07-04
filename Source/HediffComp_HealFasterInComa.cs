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
    /// Multiplier for natural healing speed. 2.0 = 2x healing rate.
    public float healFactor = 10.0f;

    /// Multiplier for addiction withdrawal/detox speed. 2.0 = 2x faster recovery.
    public float addictionHealFactor = 2.0f;

    /// Interval in ticks between healing applications. Default: 60 (1 second).
    public int intervalTicks = 60;

    public HediffCompProperties_HealFasterInComa()
    {
        this.compClass = typeof(HediffComp_HealFasterInComa);
    }
}

public class HediffComp_HealFasterInComa : HediffComp
{
    /// One in-game day = 60,000 ticks at normal speed.
    private const float TicksPerDay = 60000f;

    /// Vanilla rare-tick cadence: one rare tick every 250 game ticks.
    private const int RareTickInterval = 250;

    private HediffCompProperties_HealFasterInComa Props =>
        (HediffCompProperties_HealFasterInComa)this.props;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);

        if (this.Pawn.health?.hediffSet?.hediffs == null)
        {
            return;
        }

        // Injuries heal on the fast interval (once per second by default).
        if (this.Pawn.IsHashIntervalTick(Props.intervalTicks))
        {
            HealInjuries();
        }

        // Addictions detox on the RARE-tick cadence (every 250 ticks),
        // matching vanilla's HediffComp_SeverityPerDay, so the per-day
        // math stays correct and severity doesn't plummet.
        if (this.Pawn.IsHashIntervalTick(RareTickInterval))
        {
            HealAddictions();
        }
    }

    private void HealInjuries()
    {
        // Vanilla heals ~0.01 severity per 2500 ticks for injuries.
        // We only add the *extra* healing on top of vanilla.
        // healFactor=2.0 means 1x extra (vanilla already provides 1x).
        float extraHealAmount =
            0.01f
            * ((float)Props.intervalTicks / 2500f)
            * (Props.healFactor - 1f);

        if (extraHealAmount <= 0f) return;

        List<Hediff> hediffs = this.Pawn.health.hediffSet.hediffs;

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

    private void HealAddictions()
    {
        // Fraction of a day represented by ONE rare-tick interval.
        // (RareTickInterval / TicksPerDay) * (extra multiplier portion)
        float addictionExtraPerInterval =
            ((float)RareTickInterval / TicksPerDay)
            * (Props.addictionHealFactor - 1f);

        if (addictionExtraPerInterval <= 0f) return;

        List<Hediff> hediffs = this.Pawn.health.hediffSet.hediffs;

        for (int i = hediffs.Count - 1; i >= 0; i--)
        {
            if (i >= hediffs.Count) continue;

            if (hediffs[i] is Hediff_Addiction addiction)
            {
                HealAddiction(addiction, addictionExtraPerInterval);
            }
        }
    }

    /// Invokes the protected SeverityChangePerDay() method via reflection
    /// to obtain the vanilla detox rate for an addiction.
    private static float GetSeverityPerDay(HediffComp_SeverityPerDay severityComp)
    {
        #if !RIMWORLD12
        return severityComp.SeverityChangePerDay();
        #else
        object? result = typeof(HediffComp_SeverityPerDay)
            .GetMethod(
                "SeverityChangePerDay",
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic
            )
            ?.Invoke(severityComp, null);

        if (result is float severityPerDay)
        {
            return severityPerDay;
        }

        return 0f;
        #endif
    }

    /// Reduces an addiction's severity by the extra amount contributed by
    /// the coma. Vanilla's HediffComp_SeverityPerDay still runs normally,
    /// so the net effect is (addictionHealFactor)x recovery.
    private void HealAddiction(Hediff_Addiction addiction, float extraPerInterval)
    {
        HediffComp_SeverityPerDay? severityComp = null;
        List<HediffComp> comps = addiction.comps;
        if (comps != null)
        {
            for (int j = 0; j < comps.Count; j++)
            {
                if (comps[j] is HediffComp_SeverityPerDay spd)
                {
                    severityComp = spd;
                    break;
                }
            }
        }

        float reduction;

        if (severityComp != null)
        {
            // severityPerDay is NEGATIVE (vanilla decay direction).
            // Negate it so `reduction` is a positive amount to subtract.
            reduction = -GetSeverityPerDay(severityComp) * extraPerInterval;
        }
        else
        {
            reduction = 0.0333f * extraPerInterval;
        }

        if (reduction <= 0f) return;

        addiction.Severity -= reduction;

        if (addiction.Severity <= 0f)
        {
            this.Pawn.health.RemoveHediff(addiction);
        }
    }
}