using Verse;
using HarmonyLib;
using static CyanobotsGenes.PrecociousUtil;

namespace CyanobotsGenes
{
    //under-threes are incapable of violence even if precocious
    [HarmonyPatch(typeof(Pawn),nameof(Pawn.CombinedDisabledWorkTags), MethodType.Getter)]
    public static class CombinedDisabledWorkTags_Patch
    {
        public static void Postfix(Pawn __instance, ref WorkTags __result)
        {
            //Log.Message("Patching CombinedDisabledWorkTags - pawn: " + __instance);
            if (IsPrecociousBaby(__instance, out var _))
            {
                __result |= WorkTags.Violent;
            }
        }
    }
}
