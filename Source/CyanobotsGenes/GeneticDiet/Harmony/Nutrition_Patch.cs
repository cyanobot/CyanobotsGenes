using HarmonyLib;
using RimWorld;
using Verse;
using static CyanobotsGenes.GeneticDietUtility;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FoodUtility), "NutritionForEater")]
    class Nutrition_Patch
    {
        static void Postfix(ref float __result, Pawn eater, Thing food)
        {
            //uninterested in nonhumans
            if (eater == null || !eater.RaceProps.Humanlike) return;

            float nutritionFactor = NutritionFactorFromGeneticDiet(food, eater);
            __result *= nutritionFactor;
        }
    }

}
