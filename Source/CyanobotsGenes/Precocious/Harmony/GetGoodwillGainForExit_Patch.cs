using RimWorld;
using Verse;
using HarmonyLib;
using static CyanobotsGenes.PrecociousUtil;

namespace CyanobotsGenes
{
    //no relation gain for releasing precocious prisoners
    //to prevent farming
    //and also because how well are they really going to do if kicked out into the wild?
    [HarmonyPatch(typeof(Faction),nameof(Faction.GetGoodwillGainForExit))]
    public static class GetGoodwillGainForExit_Patch
    {
        public static void Postfix(ref int __result, Pawn member, bool freed)
        {
            if (IsPrecociousBaby(member, out var _) && freed)
            {
                __result = 0;
            }
        }
    }
}
