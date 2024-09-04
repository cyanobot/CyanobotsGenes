using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CyanobotsGenes
{
    public static class OffspringUtility
    {
        private static List<GeneDef> tmpGenes = new List<GeneDef>();
        private static List<GeneDef> tmpGenesShuffled = new List<GeneDef>();
        private static Dictionary<GeneDef, float> tmpGeneChances = new Dictionary<GeneDef, float>();

        public static bool HasActiveOffspringGene(Pawn pawn)
        {
            if (pawn?.genes == null) return false;
            if (pawn.genes.GenesListForReading.Any(g =>
                    g.Active && g.GetType() == typeof(Gene_Offspring)))
                return true;
            return false;
        }

        public static bool IsActiveOffspringGene(Gene gene)
        {
            if (gene.Active && gene.GetType() == typeof(Gene_Offspring)) return true;
            return false;
        }

        public static XenotypeDef GetOffspringXenotype(Pawn mother, Pawn father)
        {
            XenotypeDef xenotype;
            if (mother?.genes != null)
            {
                xenotype = ((Gene_Offspring)mother.genes.GenesListForReading.Find(x => IsActiveOffspringGene(x)))?.Xenotype;
                if (xenotype != null) return xenotype;
            }
            if (father?.genes != null)
            {
                xenotype = ((Gene_Offspring)father.genes.GenesListForReading.Find(x => IsActiveOffspringGene(x)))?.Xenotype;
                if (xenotype != null) return xenotype;
            }

            Log.Error("[CyanobotsGenes] Attempted to get offspring xenotype [parents: " + mother + ", " + father
                    + "] but found no Offspring gene.");
            return null;

        }

        public static List<GeneDef> GetInheritedEndogenes(Pawn mother, Pawn father, XenotypeDef overridingXenotype)
        {
            tmpGenes.Clear();
            tmpGenesShuffled.Clear();
            if (mother?.genes != null)
            {
                foreach (Gene endogene in mother.genes.Endogenes)
                {
                    if (endogene.def.endogeneCategory != EndogeneCategory.Melanin && endogene.def.biostatArc <= 0)
                    {
                        //don't inherit endogenes that are overridden by overriding xenotype
                        if (overridingXenotype.AllGenes.Any(g => g.endogeneCategory == endogene.def.endogeneCategory))
                        {
                            continue;
                        }
                        tmpGeneChances.SetOrAdd(endogene.def, 0.5f);
                        if (!tmpGenesShuffled.Contains(endogene.def))
                        {
                            tmpGenesShuffled.Add(endogene.def);
                        }
                    }
                }
            }
            if (father?.genes != null)
            {
                foreach (Gene endogene in father.genes.Endogenes)
                {
                    if (endogene.def.endogeneCategory != EndogeneCategory.Melanin && endogene.def.biostatArc <= 0)
                    {
                        //don't inherit endogenes that are overridden by overriding xenotype
                        if (overridingXenotype.AllGenes.Any(g => g.endogeneCategory == endogene.def.endogeneCategory))
                        {
                            continue;
                        }
                        tmpGeneChances.SetOrAdd(endogene.def, 0.5f);
                        if (!tmpGenesShuffled.Contains(endogene.def))
                        {
                            tmpGenesShuffled.Add(endogene.def);
                        }
                    }
                }
            }

            tmpGenes.Clear();
            tmpGenesShuffled.Shuffle();
            foreach (GeneDef item in tmpGenesShuffled)
            {
                if (!tmpGenes.Contains(item) && Rand.Chance(tmpGeneChances[item]))
                {
                    tmpGenes.Add(item);
                }
            }
            tmpGenes.RemoveAll((GeneDef x) => x.prerequisite != null && !tmpGenes.Contains(x.prerequisite));

            //don't calculate skin color if overridden by xenotype
            if (!overridingXenotype.AllGenes.Any(g => g.endogeneCategory == EndogeneCategory.Melanin
                || g.skinColorOverride != null
                || g.skinColorBase != null))
            {
                if (PawnSkinColors.SkinColorsFromParents(father, mother).TryRandomElement(out var result))
                {
                    tmpGenes.Add(result);
                }
            }

            //don't calculate hair color if overriden by xenotype
            if (!overridingXenotype.AllGenes.Any(g => g.endogeneCategory == EndogeneCategory.HairColor
                || g.hairColorOverride != null)
                && !tmpGenes.Any((GeneDef x) => x.endogeneCategory == EndogeneCategory.HairColor))
            {

                GeneDef geneDef = father?.genes?.GetFirstEndogeneByCategory(EndogeneCategory.HairColor);
                GeneDef geneDef2 = mother?.genes?.GetFirstEndogeneByCategory(EndogeneCategory.HairColor);
                GeneDef result2;
                if (geneDef != null && geneDef2 == null)
                {
                    tmpGenes.Add(geneDef);
                }
                else if (geneDef2 != null && geneDef == null)
                {
                    tmpGenes.Add(geneDef2);
                }
                else if (geneDef != null && geneDef2 != null)
                {
                    tmpGenes.Add(Rand.Bool ? geneDef2 : geneDef);
                }
                else if (DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) => x.endogeneCategory == EndogeneCategory.HairColor).TryRandomElementByWeight((GeneDef x) => x.selectionWeight, out result2))
                {
                    tmpGenes.Add(result2);
                }
            }

            if (!tmpGenes.Contains(GeneDefOf.Inbred) && Rand.Value < PregnancyUtility.InbredChanceFromParents(mother, father, out var _))
            {
                tmpGenes.Add(GeneDefOf.Inbred);
            }
            tmpGeneChances.Clear();
            tmpGenesShuffled.Clear();
            return tmpGenes;
        }
    }
}
