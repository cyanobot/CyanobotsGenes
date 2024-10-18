using HarmonyLib;
using RimWorld;
using Verse;
using static CyanobotsGenes.GeneticDietUtility;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FoodUtility),nameof(FoodUtility.GetFoodKindForStacking))]
    class GetFoodKindForStacking_Patch
    {
        static bool Prefix(ref FoodKind __result, Thing food)
        {
            if (!CG_Settings.changeMealStacking) return true;

            //pass null object issues back to vanilla/whatever other patches to be handled there
            if (food == null) return true;

            //Log.Message("Applying GetFoodKindForStacking_Patch");

            CG_FoodKind cg_foodKind = CG_FoodKindForDisplay(GetCG_FoodKind(food));
            int i = (int)cg_foodKind;
            __result = (FoodKind)i;
            //Log.Message("GetFoodKindForStacking: " + __result);
            return false;
        }
    }

}
