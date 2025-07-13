using Verse;
using HarmonyLib;
using static CyanobotsGenes.PrecociousUtil;

namespace CyanobotsGenes
{
    //precocious babies should not count as threats even if from hostile factions
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.ThreatDisabled))]
    public static class ThreatDisabled_Patch
    {
        static bool Postfix(bool result, Pawn __instance)
        {
            if (result) return true;
            if (IsPrecociousBaby(__instance, out var _)) return true;
            return false;
        }
    }
}
