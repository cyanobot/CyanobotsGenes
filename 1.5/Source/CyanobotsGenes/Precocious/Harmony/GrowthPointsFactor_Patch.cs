using Verse;
using HarmonyLib;
using System.Reflection;

namespace CyanobotsGenes
{
    [HarmonyPatch]
    public static class GrowthPointsFactor_Patch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), "GrowthPointsFactor");
        }
        public static float Postfix(float result, Pawn_AgeTracker __instance)
        {
            if ((float)__instance.AgeBiologicalYears < 3f)
            {
                result /= 0.75f;
            }
            return result;
        }
    }
}
