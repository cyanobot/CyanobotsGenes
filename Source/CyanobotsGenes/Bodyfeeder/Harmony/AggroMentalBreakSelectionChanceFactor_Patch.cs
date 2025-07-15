using HarmonyLib;
using RimWorld;
using System.Reflection;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Pawn_GeneTracker))]
    class AggroMentalBreakSelectionChanceFactor_Patch
    {
        static MethodBase TargetMethod()
        {
            return typeof(Pawn_GeneTracker).GetProperty(nameof(Pawn_GeneTracker.AggroMentalBreakSelectionChanceFactor)
                , BindingFlags.Public | BindingFlags.Instance).GetGetMethod(false);
        }

        static float Postfix(float result, Pawn_GeneTracker __instance)
        {
            if (__instance.pawn.HasActiveGene(CG_DefOf.CYB_Bodyfeeder) && __instance.pawn.health != null 
                && __instance.pawn.health.hediffSet.HasHediff(CG_DefOf.CYB_BodyfeederStarvation))
            {
                result *= 1f + (8f * __instance.pawn.health.hediffSet.GetFirstHediffOfDef(CG_DefOf.CYB_BodyfeederStarvation).Severity);
            }
            return result;
        }
    }
}
