using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

#if RW_1_5
#else

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(TransferableUtility),nameof(TransferableUtility.TransferAsOne))]
    public static class TransferableUtility_TransferAsOne_Patch
    {
        public static bool Prefix(ref bool __result, Thing a, Thing b, TransferAsOneMode mode)
        {
            //don't interfere if picky stacking is turned off
            if (!CG_Settings.changeMealStacking) return true;

            CompIngredients compIngredients = a.TryGetComp<CompIngredients>();
            CompIngredients compIngredients2 = b.TryGetComp<CompIngredients>();
            if (compIngredients != null && compIngredients2 != null
                && GeneticDietUtility.GetCG_FoodKind(a) != GeneticDietUtility.GetCG_FoodKind(b))
            {
                __result = false;
                return false;   //found reason to forbid stacking, no need for further checks
            }

            return true;    //pass back to vanilla
        }
    }
}
#endif