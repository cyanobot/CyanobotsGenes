using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Pawn_AgeTracker),nameof(Pawn_AgeTracker.TrySimulateGrowthPoints))]
    public static class TrySimulateGrowthPoints_Patch
    {
        public static void Postfix(Pawn ___pawn, Pawn_AgeTracker __instance)
        {
            //only interested in children
            if (__instance.AgeBiologicalYears > __instance.AdultMinAge) return;

            //only interested in precocious
            Gene_Precocious gene_Precocious = (Gene_Precocious)(___pawn.genes?.GetGene(CG_DefOf.CYB_Precocious));
            if (gene_Precocious == null || !gene_Precocious.Active) return;

            //only interested in would-be-babies
            float minAgeChild = gene_Precocious.MinAgeChild;
            if (__instance.AgeBiologicalYears >= (int)minAgeChild) return;

            //precocious 0-3yos also get growth points

            //private method, easier to copy than invoke
            //not really important if it's not totally accurate
            //eg not matching changes other mods might make to this
            //because this is only for simulating pawns that have not been learning in play
            float learningLevel = Rand.Range(0.2f, 0.5f);
            float growthPointsPerDayAtLearningLevel = learningLevel * __instance.ChildAgingMultiplier;
            int minAgeTicks = 0;
            long num4 = __instance.AgeBiologicalTicks;
            while (num4 > minAgeTicks)
            {
                num4 -= 60000;
                __instance.growthPoints += growthPointsPerDayAtLearningLevel;
            }
        }

    }
}
