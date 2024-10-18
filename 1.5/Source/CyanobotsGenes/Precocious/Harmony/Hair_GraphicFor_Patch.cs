using Verse;
using HarmonyLib;
using static CyanobotsGenes.PrecociousUtil;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(PawnRenderNode_Hair),nameof(PawnRenderNode_Hair.GraphicFor))]
    public static class Hair_GraphicFor_Patch
    {
        public static void Postfix(ref Graphic __result, Pawn pawn)
        {
            if (IsPrecociousBaby(pawn, out var _)) __result = null;
        }
    }
}
