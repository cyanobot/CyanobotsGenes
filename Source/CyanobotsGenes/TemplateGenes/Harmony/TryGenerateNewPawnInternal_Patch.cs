using RimWorld;
using Verse;
using HarmonyLib;
using System.Linq;

namespace CyanobotsGenes
{
    //if a pawn is being generated as a changeling/fairy by request rather than randomly based on factionlessGenerationWeight
    //then instead set them to the correct version for their developmental stage
    //so that eg factions defined with these xenotypes in produce the correct ones
    [HarmonyPatch(typeof(PawnGenerator), "TryGenerateNewPawnInternal")]
    public static class TryGenerateNewPawnInternal_Patch
    {
        public static void Postfix(ref Pawn __result)
        {
            //Log.Message("TryGenerateNewPawnInternal_Patch Postfix - __result: " + __result + ", Xenotype: " + __result?.genes?.Xenotype);
            if (__result == null) return;
            if (__result.DevelopmentalStage == DevelopmentalStage.Adult)
            {
                Gene_Metamorphosis gene_Metamorphosis = (Gene_Metamorphosis)__result.genes?.GenesListForReading.Find(g => g.GetType() == typeof(Gene_Metamorphosis) && g.Active);
                if (gene_Metamorphosis != null)
                {
                    if (!__result.genes.Endogenes.NullOrEmpty())
                    {
                        foreach (Gene gene in __result.genes.Endogenes.ToList())
                        {
                            if (gene.def.endogeneCategory == EndogeneCategory.Melanin || (gene.Active && gene.def.endogeneCategory == EndogeneCategory.HairColor))
                            {
                                continue;
                            }
                            __result.genes.RemoveGene(gene);
                        }
                    }
                    __result.genes.SetXenotype(gene_Metamorphosis.Xenotype);
                }
            }
            else if  (__result.DevelopmentalStage != DevelopmentalStage.Adult)
            {
                Gene_Offspring gene_Offspring = (Gene_Offspring)__result.genes?.GenesListForReading.Find(g => g.GetType() == typeof(Gene_Offspring) && g.Active);
                if (gene_Offspring != null)
                {
                    if (!__result.genes.Endogenes.NullOrEmpty())
                    {
                        foreach (Gene gene in __result.genes.Endogenes.ToList())
                        {
                            if (gene.def.endogeneCategory == EndogeneCategory.Melanin)
                            {
                                continue;
                            }
                            __result.genes.RemoveGene(gene);
                        }
                    }
                    __result.genes.SetXenotype(gene_Offspring.Xenotype);
                    __result.story.hairDef = PawnStyleItemChooser.RandomHairFor(__result);  //for changelings
                }
            }
        }
    }

}
