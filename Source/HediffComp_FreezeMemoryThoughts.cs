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

public class HediffCompProperties_FreezeMemoryThoughts : HediffCompProperties
{
    public HediffCompProperties_FreezeMemoryThoughts()
    {
        this.compClass = typeof(HediffComp_FreezeMemoryThoughts);
    }
}

/// <summary>
/// Keeps a pawn's mood memories from aging while the parent hediff is active.
///
/// Vanilla advances memory ages by 150 ticks per 150-tick mood interval. This
/// comp runs just before that interval and steps each age back by the same
/// amount, so vanilla's increment restores the frozen age without pushing a
/// memory past its expiration time. References to memories that have since
/// been discarded are pruned in place, with no second scan.
/// </summary>
public class HediffComp_FreezeMemoryThoughts : HediffComp
{
    /// Vanilla mood/thought interval; needs tick every 150 game ticks.
    private const int MoodInterval = 150;

    /// Age of the memories currently frozen by this comp.
    private readonly Dictionary<Thought_Memory, int> frozenAges =
        new Dictionary<Thought_Memory, int>();

    /// Scratch list used to prune frozenAges while iterating it.
    private readonly List<Thought_Memory> staleMemories =
        new List<Thought_Memory>();

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);

        // Vanilla only ages memories on the mood interval; match it so the
        // freeze stays exact without paying for this every single tick.
        if (!this.Pawn.IsHashIntervalTick(MoodInterval))
        {
            return;
        }

        List<Thought_Memory>? memories = this.Pawn
            ?.needs
            ?.mood
            ?.thoughts
            ?.memories
            ?.Memories;

        if (memories == null || memories.Count == 0)
        {
            frozenAges.Clear();
            return;
        }

        for (int i = 0; i < memories.Count; i++)
        {
            Thought_Memory memory = memories[i];

            if (frozenAges.TryGetValue(memory, out int frozenAge))
            {
                // Health ticks run before needs, so this happens before
                // vanilla's MemoryThoughtInterval. Vanilla adds MoodInterval
                // to the age and then discards anything past its duration;
                // step the age back first so that increment lands exactly on
                // the frozen age instead of expiring the memory.
                int intervalStartAge = frozenAge - MoodInterval;
                if (memory.age != intervalStartAge)
                {
                    memory.age = intervalStartAge;
                }
            }
        }

        RemoveExpiredReferences(memories);
    }

    /// Prunes entries for memories vanilla has since discarded. Runs at most
    /// once per mood interval and is O(frozen) extra lookups, not O(n * m).
    private void RemoveExpiredReferences(List<Thought_Memory> currentMemories)
    {
        if (frozenAges.Count == 0) return;

        staleMemories.Clear();

        foreach (Thought_Memory memory in frozenAges.Keys)
        {
            if (!currentMemories.Contains(memory))
            {
                staleMemories.Add(memory);
            }
        }

        for (int i = 0; i < staleMemories.Count; i++)
        {
            frozenAges.Remove(staleMemories[i]);
        }

        staleMemories.Clear();
    }

    /// Freeze whatever is in memory right now, so a memory gained later in
    /// the same mood interval isn't left un-tracked until the next one.
    private void FreezeCurrentMemories()
    {
        List<Thought_Memory>? memories = this.Pawn
            ?.needs
            ?.mood
            ?.thoughts
            ?.memories
            ?.Memories;

        if (memories == null) return;

        for (int i = 0; i < memories.Count; i++)
        {
            Thought_Memory memory = memories[i];
            if (memory != null && !frozenAges.ContainsKey(memory))
            {
                frozenAges[memory] = memory.age;
            }
        }
    }

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        FreezeCurrentMemories();
    }

    public override void CompPostMerged(Hediff other)
    {
        base.CompPostMerged(other);
        FreezeCurrentMemories();
    }

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        frozenAges.Clear();
        staleMemories.Clear();
    }
}
