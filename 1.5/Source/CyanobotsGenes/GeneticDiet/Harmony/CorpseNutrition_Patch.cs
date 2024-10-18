using HarmonyLib;
using RimWorld;
using Verse;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Corpse), "IngestedCalculateAmounts")]
    class CorpseNutrition_Patch
    {
        static void Postfix(ref float nutritionIngested, Corpse __instance, Pawn ingester)
        {
            //uninterested in nonhumans
            if (ingester == null || !ingester.RaceProps.Humanlike) return;
            nutritionIngested *= ingester.GetStatValue(CG_DefOf.AnimalNutritionFactor);
        }
    }

}
