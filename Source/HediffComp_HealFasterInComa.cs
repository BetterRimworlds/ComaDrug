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

    /// Maximum multiplier for addiction withdrawal/detox speed. 2.0 = up to 2x faster recovery.
    public float addictionHealFactor = 10.0f;

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

    /// Vanilla natural injury healing runs once per 600 ticks.
    private const int VanillaHealInterval = 600;

    /// Base severity vanilla natural healing applies per day before modifiers.
    private const float VanillaBaseHealPerDay = 8f;

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
        // Vanilla heals one random naturally-healing injury every 600 ticks.
        // Spread the configured extra fraction of that same amount over our
        // faster interval. For a single injury the net rate is healFactor
        // times vanilla. We only add the *extra* healing on top of vanilla.
        // healFactor=2.0 means 1x extra (vanilla already provides 1x).
        float extraHealAmount =
            GetVanillaHealAmount()
            * ((float)Props.intervalTicks / VanillaHealInterval)
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
                injury.Heal(extraHealAmount);
                if (injury.Severity <= 0f)
                {
                    this.Pawn.health.RemoveHediff(injury);
                }
            }
        }
    }

    /// <summary>
    /// Severity vanilla natural healing applies in one 600-tick heal tick,
    /// including posture, bed, health scale, InjuryHealingFactor, and
    /// naturalHealingFactor modifiers.
    /// </summary>
    private float GetVanillaHealAmount()
    {
        if (!this.Pawn.RaceProps.IsFlesh)
        {
            return 0f;
        }

        // Vanilla skips natural healing while the pawn is starving.
        if (this.Pawn.needs?.food != null && this.Pawn.needs.food.Starving)
        {
            return 0f;
        }

        float healPerDay = VanillaBaseHealPerDay;
        if (this.Pawn.GetPosture() != PawnPosture.Standing)
        {
            healPerDay += 4f;
            Building_Bed? bed = this.Pawn.CurrentBed();
            if (bed != null)
            {
                healPerDay += bed.def.building.bed_healPerDay;
            }
        }

        List<Hediff> hediffs = this.Pawn.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++)
        {
            HediffStage? stage = hediffs[i].CurStage;
            if (stage != null && stage.naturalHealingFactor != -1f)
            {
                healPerDay *= stage.naturalHealingFactor;
            }
        }

        float injuryHealingFactor = 1f;
#if !RIMWORLD12
        injuryHealingFactor = this.Pawn.GetStatValue(StatDefOf.InjuryHealingFactor);
#endif

        return healPerDay
            * this.Pawn.HealthScale
            * 0.01f
            * injuryHealingFactor;
    }

    private void HealAddictions()
    {
        // Randomize from 0 to full (Props.addictionHealFactor) in steps of 0.01
        int maxSteps = (int)System.Math.Round(Props.addictionHealFactor * 100f);
        float randomizedFactor = Verse.Rand.RangeInclusive(0, maxSteps) / 100f;

        // Fraction of a day represented by ONE rare-tick interval.
        // (RareTickInterval / TicksPerDay) * (extra multiplier portion)
        float addictionExtraPerInterval =
            ((float)RareTickInterval / TicksPerDay)
            * (randomizedFactor - 1f);

        // If the randomized factor rolled <= 1.0, there is no *extra* healing
        // to apply on top of vanilla for this interval tick.
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
    /// so the net effect is (addictionHealFactor)x recovery. Addictions
    /// without a natural recovery rate (Luciferium, Luciferium-addict
    /// mechanitors, etc.) are left untouched: they never decay in vanilla,
    /// so speeding them up would permanently cure the dependence.
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

        if (severityComp == null) return;

        // severityPerDay is NEGATIVE (vanilla decay direction).
        // Negate it so `reduction` is a positive amount to subtract.
        float reduction = -GetSeverityPerDay(severityComp) * extraPerInterval;

        if (reduction <= 0f) return;

        addiction.Severity -= reduction;

        if (addiction.Severity <= 0f)
        {
            this.Pawn.health.RemoveHediff(addiction);
        }
    }
}
