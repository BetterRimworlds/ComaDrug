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

public class HediffComp_FreezeMemoryThoughts : HediffComp
{
    private readonly Dictionary<Thought_Memory, int> frozenAges = new Dictionary<Thought_Memory, int>();

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);

        List<Thought_Memory>? memories = this.Pawn
            ?.needs
            ?.mood
            ?.thoughts
            ?.memories
            ?.Memories;

        if (memories == null)
        {
            frozenAges.Clear();
            return;
        }

        RemoveExpiredReferences(memories);

        for (int i = 0; i < memories.Count; i++)
        {
            Thought_Memory memory = memories[i];

            if (!frozenAges.TryGetValue(memory, out int frozenAge))
            {
                frozenAges[memory] = memory.age;
                continue;
            }

            if (memory.age != frozenAge)
            {
                memory.age = frozenAge;
            }
        }
    }

    private void RemoveExpiredReferences(List<Thought_Memory> currentMemories)
    {
        if (frozenAges.Count == 0) return;

        List<Thought_Memory>? staleMemories = null;

        foreach (Thought_Memory memory in frozenAges.Keys)
        {
            if (!currentMemories.Contains(memory))
            {
                staleMemories ??= new List<Thought_Memory>();
                staleMemories.Add(memory);
            }
        }

        if (staleMemories == null) return;

        for (int i = 0; i < staleMemories.Count; i++)
        {
            frozenAges.Remove(staleMemories[i]);
        }
    }
}
