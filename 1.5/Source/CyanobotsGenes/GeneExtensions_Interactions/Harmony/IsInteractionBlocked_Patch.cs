using RimWorld;
using Verse;
using HarmonyLib;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(PawnUtility),nameof(PawnUtility.IsInteractionBlocked))]
    public static class IsInteractionBlocked_Patch
    {
        public static bool Postfix(bool __result, Pawn pawn, InteractionDef interaction, bool isInitiator)
        {
            if (__result) return true;
            if (pawn.genes == null || pawn.genes.GenesListForReading.NullOrEmpty()) return __result;
            foreach (Gene gene in pawn.genes.GenesListForReading)
            {
                if (!gene.Active) continue;
                GeneExtension_BlocksInteractions blocksInteractions = gene.def.GetModExtension<GeneExtension_BlocksInteractions>();
                if (blocksInteractions == null) continue;
                if (isInitiator && blocksInteractions.BlocksAsInitiator(interaction)) return true;
                if (!isInitiator && blocksInteractions.BlocksAsRecipient(interaction)) return true;
            }
            return false;
        }
    }

}
