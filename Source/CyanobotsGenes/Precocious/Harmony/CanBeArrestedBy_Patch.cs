using Verse;
using HarmonyLib;
using static CyanobotsGenes.PrecociousUtil;
using Verse.AI;

namespace CyanobotsGenes
{
    //precocious babies are arrestable
    [HarmonyPatch(typeof(GenAI), nameof(GenAI.CanBeArrestedBy))]
    public static class CanBeArrestedBy_Patch
    {
        static bool Postfix(bool __result, Pawn pawn)
        {
            if (__result == true) return true;
            if (IsPrecociousBaby(pawn, out var _))
            {
                if (pawn.IsMutant)
                {
                    return false;
                }
                if (pawn.IsPrisonerOfColony && pawn.Position.IsInPrisonCell(pawn.MapHeld))
                {
                    return false;
                }
                if (ModsConfig.AnomalyActive && Find.Anomaly.IsPawnHypnotized(pawn))
                {
                    return false;
                }
                return true;
            }
            return __result;
        }
    }
}
