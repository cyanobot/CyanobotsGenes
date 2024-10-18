using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using static CyanobotsGenes.GeneticDietUtility;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Graphic_MealVariants),nameof(Graphic_MealVariants.SubGraphicTypeIndex))]
    class MealVariants_SubGraphicTypeIndex_Patch
    {
        static bool Prefix(ref int __result, Thing thing)
        {
            //pass any null object problems back to vanilla
            if (thing == null) return true;

            //only fuck with vanilla meals
            List<string> mealDefNames = new List<string>
            {
                "MealSurvivalPack",
                "MealSimple",
                "MealFine",
                "MealFine_Veg",
                "MealLavish",
                "MealLavish_Veg"
            };
            if (!mealDefNames.Contains(thing.def.defName)) return true;

            if (CG_Settings.changeMealStacking)
            {

                CG_FoodKind foodKind = GetCG_FoodKind(thing);
                __result = (int)foodKind;
                /*
                CG_FoodKind displayKind = CG_FoodKindForDisplay(GetCG_FoodKind(thing));
                switch (displayKind)
                {
                    case (CG_FoodKind.Meat):
                        __result = 0;
                        break;
                    case (CG_FoodKind.Vegetable):
                        __result = 1;
                        break;
                    case (CG_FoodKind.Any):
                        __result = 2;
                        break;
                    case (CG_FoodKind.AnimalProduct):
                        __result = 3;
                        break;
                    case (CG_FoodKind.AnimalProductAndVeg):
                        __result = 4;
                        break;
                    case (CG_FoodKind.MeatAndVeg):
                        __result = 5;
                        break;
                    default:
                        __result = 2;
                        break;
                }
                */
            }
            else
            {
                FoodKind vanilla_foodKind = FoodUtility.GetFoodKind(thing);
                switch (vanilla_foodKind)
                {
                    case (FoodKind.Any): 
                        __result = 0;
                        break;
                    case (FoodKind.Meat): 
                        __result = 1;
                        break;
                    case (FoodKind.NonMeat): 
                        __result = 2;
                        break;
                    default: 
                        __result = 0;
                        break;
                }
                //Log.Message("vanilla_foodKind: " + vanilla_foodKind + ", __result: " + __result);
            }
            __result *= 3;
            //Log.Message("SubGraphicIndex: " + __result);
            return false;
        }
    }

}
