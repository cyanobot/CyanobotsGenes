using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.WillEat), new Type[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool), typeof(bool) })]
    class WillEat_Patch
    {
        static bool Postfix(bool __result, Pawn p, ThingDef food)
        {
            if (!p.RaceProps.Humanlike || p.genes == null) return __result;
            if (!__result) return false;
            else return !GeneticDietUtility.DietForbids(food, p);
        }
    }

}
