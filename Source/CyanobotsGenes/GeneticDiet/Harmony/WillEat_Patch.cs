using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.WillEat), new Type[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool), typeof(bool) })]
    class WillEatDef_Patch
    {
        static bool Postfix(bool __result, Pawn p, ThingDef food)
        {
            //LogUtil.DebugLog($"WillEat(def) Postfix - pawn: {p}, food: {food}, result: {__result}" +
            //    $", humanlike: {p.RaceProps.Humanlike}, genes: {p.genes}"
            //    );

            if (!__result) return false;
            if (!p.RaceProps.Humanlike || p.genes == null) return __result;
            else return !GeneticDietUtility.DietForbids(food, p);
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.WillEat), new Type[] { typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool) })]
    class WillEatThing_Patch
    {
        static bool Postfix(bool __result, Pawn p, Thing food)
        {
            //LogUtil.DebugLog($"WillEat(thing) Postfix - pawn: {p}, food: {food}, result: {__result}" +
            //    $", humanlike: {p.RaceProps.Humanlike}, genes: {p.genes}"
            //    );

            if (!__result) return false;
            if (!p.RaceProps.Humanlike || p.genes == null) return __result;
            else return !GeneticDietUtility.DietForbids(food, p);
        }
    }
}
