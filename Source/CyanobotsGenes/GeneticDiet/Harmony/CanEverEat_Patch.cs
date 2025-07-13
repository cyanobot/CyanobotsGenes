using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(RaceProperties), nameof(RaceProperties.CanEverEat), new Type[] { typeof(ThingDef) })]
    class CanEverEat_Patch
    {
        static bool Postfix(bool __result, RaceProperties __instance, ThingDef t)
        {
            if (__result || !__instance.Humanlike) return __result;
            if (t == ThingDefOf.Hay) return true;
            return false;
        }
    }

}
