using HarmonyLib;
using Verse;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Thing), "Ingested")]
    class Indigestion_Patch
    {
        static void Postfix(Pawn ingester, Thing __instance)
        {
            if (!ingester.RaceProps.Humanlike || ingester.genes == null) return;

            GeneticDietUtility.AddIndigestionHediff(ingester, __instance);
        }
    }

}
