using RimWorld;
using Verse;
using HarmonyLib;
using System;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new Type[] { typeof(Thought_Memory), typeof(Pawn) })]
    class TryGainMemory_Patch
    {
        static bool Prefix(ref Thought_Memory newThought, Pawn otherPawn, Pawn ___pawn, MemoryThoughtHandler __instance)
        {
            //Log.Message("TryGainMemory - newThought: " + newThought.def + ", ___pawn: " + ___pawn);
            if (___pawn.genes == null) return true; ;

            if (newThought.def == ThoughtDefOf.SoakingWet && ___pawn.HasActiveGene(CG_DefOf.LightFur))
            {
                __instance.TryGainMemoryFast(CG_DefOf.WetFur);
                return true;
            }

            if (___pawn.HasActiveGene(CG_DefOf.CYB_Afamilial))
            {
                string defName = newThought.def.defName;
                if (newThought.def == ThoughtDefOf.MyCryingBaby)
                {
                    newThought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.CryingBaby);
                    return true;
                }
                else if (defName == "SoldMyLovedOne")
                {
                    if (___pawn.relations.OpinionOf(otherPawn) >= Pawn_RelationsTracker.FriendOpinionThreshold)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (___pawn.HasActiveGene(CG_DefOf.CYB_ViolenceAverse))
            {
                string defName = newThought.def.defName;
                if (newThought.def == ThoughtDefOf.WitnessedDeathAlly)
                {
                    newThought = (Thought_Memory)ThoughtMaker.MakeThought(CG_DefOf.CYB_ViolenceAverse_WitnessedDeathAlly);
                    return true;
                }
                else if (newThought.def == ThoughtDefOf.WitnessedDeathNonAlly) 
                {
                    newThought = (Thought_Memory)ThoughtMaker.MakeThought(CG_DefOf.CYB_ViolenceAverse_WitnessedDeathNonAlly);
                    return true;
                }
                else if (defName == "KnowGuestOrganHarvested" || defName == "KnowColonistOrganHarvested")
                {
                    newThought = (Thought_Memory)ThoughtMaker.MakeThought(CG_DefOf.CYB_ViolenceAverse_KnowOrganHarvested);
                    return true;
                }
                else if (newThought.def == ThoughtDefOf.HarmedMe)
                {
                    newThought = (Thought_Memory)ThoughtMaker.MakeThought(CG_DefOf.CYB_ViolenceAverse_HarmedMe);
                    return true;
                }
            }

            return true;
        }
    }

}
