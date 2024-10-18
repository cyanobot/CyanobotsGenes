using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using static CyanobotsGenes.CG_Mod;

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

            bool motherDominant = HasDominantGene(mother);
            bool fatherDominant = HasDominantGene(father);
            bool motherRecessive = HasRecessiveGene(mother);
            bool fatherRecessive = HasRecessiveGene(father);

            //no dominance relation / equivalent dominance
            if (!vreHighmateLoaded
                || (!(motherDominant ^ fatherDominant) && (!(motherRecessive ^ fatherRecessive))))
            {
                //mother gets first shot at determining offspring xenotype
                if (mother?.genes != null)
                {
                    xenotype = GetOffspringXenotype(mother);
                    if (xenotype != null) return xenotype;
                }
                //if mother doesn't have an Offspring gene, father's overrides her genes
                if (father?.genes != null)
                {
                    xenotype = GetOffspringXenotype(father);
                    if (xenotype != null) return xenotype;
                }
            }

            //mother dominant over father
            else if ((motherDominant && !fatherDominant)
                || (fatherRecessive && !motherRecessive))
            {
                if (mother?.genes != null)
                {
                    xenotype = GetOffspringXenotype(mother);
                    if (xenotype != null) return xenotype;
                }
                return null;    //dominant mother's regular genes (inheritance defined in VRE Highmate) override father's potential Offspring gene
            }

            //father dominance over mother
            else
            {
                if (father?.genes != null)
                {
                    xenotype = GetOffspringXenotype(father);
                    if (xenotype != null) return xenotype;
                }

                return null;    //dominant father's regular genes (inheritance defined in VRE Highmate) override mother's potential Offspring gene
            }
                              
            //falling through indicates neither parent has Offspring gene, should  never have called this method
            Log.Error("[CyanobotsGenes] Attempted to get offspring xenotype [parents: " + mother + ", " + father
                    + "] but found no Offspring gene.");
            return null;

        }

        public static XenotypeDef GetOffspringXenotype(Pawn parent)
        {
            if (parent?.genes == null)
                Log.Error("[CyanobotsGenes] Attempted to get offspring xenotype from invalid parent: " + parent);

            List<XenotypeDef> potentialXenotypes = new List<XenotypeDef>(); 
            
            foreach (Gene xenogene in parent.genes.Xenogenes)
            {
                if (IsActiveOffspringGene(xenogene))
                {
                    potentialXenotypes.Add(((Gene_Offspring)xenogene).Xenotype);
                }
            }
            if (potentialXenotypes.Count == 0)
            {
                foreach (Gene endogene in parent.genes.Endogenes)
                {
                    if (IsActiveOffspringGene(endogene))
                    {
                        potentialXenotypes.Add(((Gene_Offspring)endogene).Xenotype);
                    }
                }
            }

            if (potentialXenotypes.Count > 0) return potentialXenotypes.RandomElement();

            //falling through indicates no Offspring gene found
            return null;
        }

        public static bool HasDominantGene(Pawn pawn)
        {
            if (pawn == null) return false;
            if (!vreHighmateLoaded) return false;
            if (pawn.genes == null) return false;
            if (pawn.genes.GenesListForReading.Any(
                g => g.Active && g.def.defName == "VRE_DominantGenome"
                ))
            {
                return true;
            }
            return false;
        }

        public static bool HasRecessiveGene(Pawn pawn)
        {
            if (pawn == null) return false;
            if (!vreHighmateLoaded) return false;
            if (pawn.genes == null) return false;
            if (pawn.genes.GenesListForReading.Any(
                g => g.Active && g.def.defName == "VRE_RecessiveGenome"
                ))
            {
                return true;
            }
            return false;
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
