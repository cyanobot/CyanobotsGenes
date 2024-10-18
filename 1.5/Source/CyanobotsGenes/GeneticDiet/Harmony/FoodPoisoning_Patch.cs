using HarmonyLib;
using RimWorld;
using Verse;
using static CyanobotsGenes.GeneticDietUtility;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.AddFoodPoisoningHediff))]
    class FoodPoisoning_Patch
    {
        static bool Prefix(Pawn pawn, Thing ingestible, FoodPoisonCause cause)
        {
            //Log.Message("Firing FoodPoisoning_Patch.Prefix, pawn: " + pawn + ", ingestible: " + ingestible + ", cause: " + cause);
            if (cause == FoodPoisonCause.DangerousFoodType)
            {
                if (BodyfeederUtility.IsBodyFeeder(pawn) && FoodUtility.IsHumanlikeCorpseOrHumanlikeMeat(ingestible, ingestible.def))
                {
                    //Log.Message("Bodyfeeder eating humanlike");
                    return false;
                }
                DietCategory dietCategory = GetDietCategory(pawn);
                if (dietCategory == DietCategory.Hypercarnivore && ingestible.def.IsMeat)
                {
                    //Log.Message("Hypercarnivore eating meat");
                    return false;
                }
                if (dietCategory == DietCategory.StrictHerbivore && !HasFlags_CarnivoreOnly(ingestible.def))
                {
                    //Log.Message("Strict herbivore eating plants");
                    return false;
                }
            }
            //Log.Message("Returning to vanilla...");
            return true;
        }
    }

}
