using RimWorld;
using Verse;
using HarmonyLib;
using static CyanobotsGenes.PrecociousUtil;

namespace CyanobotsGenes
{
    //precocious babies are adoptable
    [HarmonyPatch(typeof(Pawn),nameof(Pawn.AdoptableBy))]
    public static class AdoptableBy_Patch
    {
        static bool Postfix(bool __result, Pawn __instance, Faction by)
        {
            if (__result == true) return true;
            if (IsPrecociousBaby(__instance, out var _))
            {
                if (__instance.Faction == by)
                {
                    return false;
                }
#if RW_1_5
                if (__instance.FactionPreventsClaimingOrAdopting(__instance.Faction, forClaim: false))
#else
                if (__instance.FactionPreventsClaimingOrAdopting(__instance.Faction, forClaim: false, out _))
#endif
                {
                    return false;
                }
                return true;
            }
            return __result;
        }
    }
}
