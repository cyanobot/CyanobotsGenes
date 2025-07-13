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
        public const bool OFFSPRING_LOG = false;

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
            Pawn primary = PrimaryParent(mother, father);
            if (primary == null) return null;
            XenotypeDef xenotype = GetOffspringXenotype(primary);

            return xenotype;
        }

        public static XenotypeDef GetOffspringXenotype(Pawn parent)
        {
            if (parent?.genes == null)
                Log.Error("[CyanobotsGenes] Attempted to get offspring xenotype from invalid parent: " + parent);

            List<XenotypeDef> potentialXenotypes = GetPotentialOffspringXenotypes(parent);

            if (potentialXenotypes.Count > 0) return CasteUtility.SelectXenotypeFromOptions(potentialXenotypes);

            //falling through indicates no Offspring gene found
            return null;
        }

        /*
        public static XenotypeDef GetXenotypeForGeneratedOffspring(Pawn mother, Pawn father, List<GeneDef> genes)
        {
            List<XenotypeDef> potentialXenotypes = GetPotentialOffspringXenotypes(mother, father);
            foreach (XenotypeDef xenotype in potentialXenotypes)
            {
                if (genes.SequenceEqual(xenotype.AllGenes))
                {
                    return xenotype;
                }
            }

            LogUtil.OffspringLog("Failed to GetXenotypeForGeneratedOffspring - mother: " + mother
                + ", father: " + father
                + ", genes: " + genes.ToStringSafeEnumerable());
            return null;
        }
        */       

        public static Pawn PrimaryParent(Pawn mother, Pawn father)
        {
            bool motherDominant = HasDominantGene(mother);
            bool fatherDominant = HasDominantGene(father);
            bool motherRecessive = HasRecessiveGene(mother);
            bool fatherRecessive = HasRecessiveGene(father);

            LogUtil.OffspringLog("PrimaryParent - mother: " + mother
                + ", father: " + father
                + ", motherDominant: " + motherDominant
                + ", fatherDominant: " + fatherDominant
                + ", motherRecessive: " + motherRecessive
                + ", fatherRecessive: " + fatherRecessive
                + ", vreHighmateLoaded: " + vreHighmateLoaded
                + ", betterGeneInheritanceLoaded: " + betterGeneInheritanceLoaded
                );

            //no dominance relation / equivalent dominance
            if ((!vreHighmateLoaded && !betterGeneInheritanceLoaded)
                || (!(motherDominant ^ fatherDominant) && (!(motherRecessive ^ fatherRecessive))))
            {
                LogUtil.OffspringLog("no/equivalent dominance");
                //mother gets first shot at determining offspring xenotype
                if (HasActiveOffspringGene(mother))
                {
                    return mother;
                }
                //if mother doesn't have an Offspring gene, father's overrides her genes
                if (HasActiveOffspringGene(father))
                {
                    return father;
                }
                //fall-through
                return null;
            }

            //mother dominant over father
            else if ((motherDominant && !fatherDominant)
                || (fatherRecessive && !motherRecessive))
            {
                LogUtil.OffspringLog("mother dominant");
                if (HasActiveOffspringGene(mother))
                {
                    return mother;
                }
                return null;    //dominant mother's regular genes override father's potential Offspring gene
            }

            //father dominance over mother
            else
            {
                LogUtil.OffspringLog("father dominant");
                if (HasActiveOffspringGene(father))
                {
                    return father;
                }

                return null;    //dominant father's regular genes override mother's potential Offspring gene
            }
        }

        public static List<XenotypeDef> GetPotentialOffspringXenotypes(Pawn mother, Pawn father)
        {
            Pawn primary = PrimaryParent(mother, father);
            return GetPotentialOffspringXenotypes(primary);
        }

        public static List<XenotypeDef> GetPotentialOffspringXenotypes(Pawn parent)
        {
            List<XenotypeDef> potentialXenotypes = new List<XenotypeDef>();

            if (parent?.genes == null)
                //occurs when a dominant partner overrides offspring gene
                //also potentially in other edge cases
                //return empty list
                return potentialXenotypes;

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

            return potentialXenotypes;
        }

        public static bool HasDominantGene(Pawn pawn)
        {
            if (pawn == null) return false;
            if (!vreHighmateLoaded && !betterGeneInheritanceLoaded) return false;
            if (pawn.genes == null) return false;
            if (pawn.genes.GenesListForReading.Any(
                g => g.Active && 
                (g.def.defName == "VRE_DominantGenome" || g.def.defName == "BGI_DominantGenes")
                ))
            {
                return true;
            }
            return false;
        }

        public static bool HasRecessiveGene(Pawn pawn)
        {
            if (pawn == null) return false;
            if (!vreHighmateLoaded && !betterGeneInheritanceLoaded) return false;
            if (pawn.genes == null) return false;
            if (pawn.genes.GenesListForReading.Any(
                g => g.Active && 
                (g.def.defName == "VRE_RecessiveGenome" || g.def.defName == "BGI_RecessiveGenes")
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

            /*
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
            */

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

            LogUtil.OffspringLog("InbredChanceFromParents: " + PregnancyUtility.InbredChanceFromParents(mother, father, out var _));
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
