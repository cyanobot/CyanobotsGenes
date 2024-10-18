using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;

namespace CyanobotsGenes
{
    [HarmonyPatch]
    public static class UnnaturalDarkness_Patch
    {
        public static bool Prepare(MethodBase original)
        {
            if (!ModLister.AnomalyInstalled) return false;
            return true;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(GameCondition_UnnaturalDarkness), nameof(GameCondition_UnnaturalDarkness.InUnnaturalDarkness));
        }

        public static bool Postfix(bool __result, Pawn p)
        {
            if (p.HasActiveGene(CG_DefOf.CYB_Darkling)) return false;
            return __result;
        }
    }

}
