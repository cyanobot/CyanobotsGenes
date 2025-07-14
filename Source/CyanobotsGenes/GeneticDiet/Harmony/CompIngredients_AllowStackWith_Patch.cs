using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using static HarmonyLib.Code;

#if RW_1_5
#else
namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(CompIngredients),nameof(CompIngredients.AllowStackWith))]
    public static class CompIngredients_AllowStackWith_Patch
    {
        public static bool Prefix(ref bool __result, CompIngredients __instance, Thing otherStack)
        {
            //don't interfere if picky stacking is turned off
            if (!CG_Settings.changeMealStacking) return true;

            //basic checks borrowed from vanilla
            if (!otherStack.TryGetComp(out CompIngredients comp))
            {
                return true;    //not much point having vanilla check again, but in case other mods also want to run code
            }
            if (!__instance.Props.performMergeCompatibilityChecks || !comp.Props.performMergeCompatibilityChecks)
            {
                return true;  //not much point having vanilla check again, but in case other mods also want to run code
            }

            if (GeneticDietUtility.GetCG_FoodKind(__instance.parent) != GeneticDietUtility.GetCG_FoodKind(otherStack))
            {
                __result = false;
                return false;   //found reason to forbid stacking, no need for further checks
            }

            return true;    //pass back to vanilla
        }
    }
}
#endif