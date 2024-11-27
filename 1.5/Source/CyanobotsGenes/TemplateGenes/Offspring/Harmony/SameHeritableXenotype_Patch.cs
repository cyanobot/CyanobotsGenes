using HarmonyLib;
using Verse;
using RimWorld;
using static CyanobotsGenes.OffspringUtility;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(GeneUtility), nameof(GeneUtility.SameHeritableXenotype))]
    public static class SameHeritableXenotype_Patch
    {
        public static bool Prefix(ref bool __result, Pawn pawn1, Pawn pawn2)
        {
            //Log.Message("Running SameHeritableXenotype_Patch");
            if (HasActiveOffspringGene(pawn1) || HasActiveOffspringGene(pawn2))
            {
                //Log.Message("SameHeritableXenotype_Patch returning false");
                __result = false;
                return false;
            }
            return true;
        }
    }

}
