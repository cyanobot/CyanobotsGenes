using RimWorld;
using Verse;
using HarmonyLib;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Need_Joy), "FallPerInterval", MethodType.Getter)]
    class Need_Joy_Patch
    {
        static void Postfix(ref float __result, Need_Joy __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;
            float JoyFallFactor = pawn.GetStatValue(CG_DefOf.CYB_JoyFallRateFactor, true, -1);
            __result *= JoyFallFactor;
        }
    }

}
