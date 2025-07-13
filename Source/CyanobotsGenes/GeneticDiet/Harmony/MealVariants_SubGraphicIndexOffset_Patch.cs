using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CyanobotsGenes
{

    [HarmonyPatch(typeof(Graphic_MealVariants),nameof(Graphic_MealVariants.SubGraphicIndexOffset))]
    class MealVariants_SubGraphicIndexOffset_Patch
    {
        static bool Prefix(ref int __result, Thing thing)
        {
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

            if (thing.stackCount == 1) __result = 0;
            else if (thing.stackCount == thing.def.stackLimit) __result = 2;
            else __result = 1;

            //Log.Message("SubGraphicIndexOffset: " + __result);
            return false;
        }
    }

}
