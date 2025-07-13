using HarmonyLib;
using RimWorld;
using Verse;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FoodUtility), "TryFindBestFoodSourceFor")]
    class TryFindBestFoodSourceFor_Patch
    {
        static void Prefix(ref bool allowCorpse, bool desperate, Pawn eater)
        {
            if (allowCorpse || !eater.RaceProps.Humanlike || eater.genes == null) return;

            if (GeneticDietUtility.GetDietCategory(eater) == DietCategory.Hypercarnivore /*|| GeneticDietUtility.IsGeneticCannibal(eater)*/)
            {
                allowCorpse = desperate;
            }
        }
    }

}
