using RimWorld;
using Verse;
using HarmonyLib;
using System.Linq;

namespace CyanobotsGenes
{
    //if a xenotype has metamorphosis, it should only generate as babies/children
    //if it has an offspring gene, it should only generate as adults
    //if tried to generate a factionless otherwise, reset to baseliner
    //so that xenotypes are not generated above the factionlessGenerationWeight from settings
    [HarmonyPatch(typeof(PawnGenerator),nameof(PawnGenerator.AdjustXenotypeForFactionlessPawn))]
    public static class AdjustXenotypeForFactionlessPawn_Patch
    {
        public static void Postfix(Pawn pawn, ref PawnGenerationRequest request, ref XenotypeDef xenotype)
        {
            if (pawn.DevelopmentalStage == DevelopmentalStage.Adult && xenotype.AllGenes.Any(g => g.geneClass == typeof(Gene_Metamorphosis)))
            {
                xenotype = XenotypeDefOf.Baseliner;
            }
            else if (pawn.DevelopmentalStage != DevelopmentalStage.Adult && xenotype.AllGenes.Any(g => g.geneClass == typeof(Gene_Offspring)))
            {
                xenotype = XenotypeDefOf.Baseliner;
            }
        }
    }

}
