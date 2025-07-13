using HarmonyLib;
using RimWorld;
using Verse;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.FoodOptimality))]
    class FoodOptimality_Patch
    {
        static void Postfix(ref float __result, Pawn eater, Thing foodSource)
        {
            if (!eater.RaceProps.Humanlike || eater.genes == null) return;

            __result += GeneticDietUtility.OptimalityOffsetFromGeneticDiet(eater, foodSource);
        }
    }

}
