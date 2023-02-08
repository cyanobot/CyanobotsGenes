using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(JobGiver_GetHemogen), "TryGiveJob")]
    class GetHemogen_Patch
    {
        static bool Prefix(ref Job __result, Pawn pawn)
        {
            if (pawn.genes == null || !pawn.genes.HasGene(CG_DefOf.Bodyfeeder))
                return true;

            Gene_Hemogen gene_Hemogen = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            if (gene_Hemogen == null)
            {
                __result = null;
                return false;
            }
            if (!gene_Hemogen.ShouldConsumeHemogenNow())
            {
                __result = null;
                return false;
            }
            Thing food = BodyfeederUtility.TryGetHumanlikeFood(pawn, false);
            if (food == null && pawn.health.hediffSet.HasHediff(HediffDefOf.HemogenCraving))
                food = BodyfeederUtility.TryGetHumanlikeFood(pawn, true);

            //if we couldn't find any human meat to eat, pass back to vanilla
            //and consider eg hemogen packs
            if (food == null)
            {
                __result = null;
                return true;
            }

            Pawn holder = (food.ParentHolder as Pawn_InventoryTracker)?.pawn;
            if (holder != null && holder != pawn)
            {
                Job getFood = JobMaker.MakeJob(JobDefOf.TakeFromOtherInventory, food, holder);
                getFood.count = BodyfeederUtility.NumToIngestForHemogen(pawn, food);
                __result = getFood;
                return false;
            }
            Job jobIngest = JobMaker.MakeJob(CG_DefOf.IngestForHemogen, food);
            jobIngest.count = BodyfeederUtility.NumToIngestForHemogen(pawn, food);
            __result = jobIngest;
            return false;
        }

    }

    [HarmonyPatch(typeof(Pawn_GeneTracker))]
    static class AggroMentalBreakSelectionChanceFactor_Patch
    {
        static MethodBase TargetMethod()
        {
            return typeof(Pawn_GeneTracker).GetProperty(nameof(Pawn_GeneTracker.AggroMentalBreakSelectionChanceFactor)
                , BindingFlags.Public | BindingFlags.Instance).GetGetMethod(false);
        }

        static float Postfix(float result, Pawn_GeneTracker __instance)
        {
            if (__instance.HasGene(CG_DefOf.Bodyfeeder) && __instance.pawn.health != null 
                && __instance.pawn.health.hediffSet.HasHediff(CG_DefOf.BodyfeederStarvation))
            {
                result *= 1f + (8f * __instance.pawn.health.hediffSet.GetFirstHediffOfDef(CG_DefOf.BodyfeederStarvation).Severity);
            }
            return result;
        }
    }

    [HarmonyPatch(typeof(GeneResourceDrainUtility),nameof(GeneResourceDrainUtility.PostResourceOffset))]
    static class PostResourceOffset_Patch
    {
        public static bool Prefix(IGeneResourceDrain drain, float oldValue)
        {
            Pawn pawn = drain.Pawn;
            if (drain.Resource.GetType() == typeof(Gene_Hemogen) && pawn.genes.HasGene(CG_DefOf.Bodyfeeder))
            {
                if (oldValue > 0f && drain.Resource.Value <= 0f)
                {
                    if (!pawn.health.hediffSet.HasHediff(CG_DefOf.BodyfeederStarvation))
                    {
                        pawn.health.AddHediff(CG_DefOf.BodyfeederStarvation);
                    }
                }
                return false;
            }
            return true;
        }
    }
}
