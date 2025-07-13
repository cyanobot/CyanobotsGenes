using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using static CyanobotsGenes.GeneticDietUtility;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(CompIngredients),"GetFoodKindInspectString")]
    class GetFoodKindInspectString_Patch
    {
        static bool Prefix(ref string __result, CompIngredients __instance)
        {
            //pass any null object problems back to vanilla to be handled there
            if (__instance == null | __instance.parent == null) return true;

            CG_FoodKind displayKind = CG_FoodKindForDisplay(GetCG_FoodKind(__instance.parent));

            switch (displayKind)
            {
                case (CG_FoodKind.AnimalProduct):
                    __result = "CYB_AnimalProduct".Translate().Colorize(ColorLibrary.Peach);
                    break;
                case (CG_FoodKind.AnimalProductAndVeg):
                    __result = "CYB_AnimalProduct".Translate().Colorize(ColorLibrary.Peach) + " + " + "CYB_Veg".Translate().Colorize(Color.green);
                    break;
                case (CG_FoodKind.Any):
                    __result = "MealKindAny".Translate().Colorize(Color.white);
                    break;
                case (CG_FoodKind.Meat):
                    __result = "CYB_Meat".Translate().Colorize(ColorLibrary.RedReadable);
                    break;
                case (CG_FoodKind.MeatAndVeg):
                    __result = "CYB_Meat".Translate().Colorize(ColorLibrary.RedReadable) + " + " + "CYB_Veg".Translate().Colorize(Color.green);
                    break;
                case (CG_FoodKind.Vegetable):
                    __result = "CYB_Vegan".Translate().Colorize(Color.green);
                    break;
                default:
                    //if we fall through, default back to vanilla behaviour
                    return true;
            }
            return false;
        }
    }

}
