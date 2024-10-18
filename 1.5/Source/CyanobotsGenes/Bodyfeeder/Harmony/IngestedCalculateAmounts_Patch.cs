using HarmonyLib;
using Verse;
using static CyanobotsGenes.BodyfeederUtility;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Corpse), "IngestedCalculateAmounts")]
    class IngestedCalculateAmounts_Patch
    {
        public static void Postfix(float nutritionIngested, Pawn ingester, Corpse __instance)
        {
            if (IsBodyFeeder(ingester))
            {
                ingester.genes.GetFirstGeneOfType<Gene_Bodyfeeder>().Notify_IngestedCorpse(__instance, nutritionIngested);
            }
        }
    }
}
