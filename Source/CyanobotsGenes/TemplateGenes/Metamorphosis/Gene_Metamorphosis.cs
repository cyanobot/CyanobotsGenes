using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace CyanobotsGenes
{
    class Gene_Metamorphosis : Gene
    {
        public static MethodInfo m_EnsureCorrectSkinColorOverride = AccessTools.Method(typeof(Pawn_GeneTracker), "EnsureCorrectSkinColorOverride");

        public const int tickInterval = 2500;       //1 hour
        public XenotypeDef Xenotype => this.def.GetModExtension<GeneExtension_Xenotype>()?.xenotype;

        //public bool alreadyMetamorphosed = false;
        public bool? chosenGene = null;

        public override bool Active
        {
            get
            {
                if (!base.Active) return false;
                if (chosenGene == false) return false;
                return true;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(tickInterval) && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
            {
                TriggerMetamorphosis();
            }
        }

        public void TriggerMetamorphosis()
        {
            if (chosenGene != null) return;

            List<Gene_Metamorphosis> geneOptions = new List<Gene_Metamorphosis>();

            foreach (Gene xenogene in pawn.genes.Xenogenes)
            {
                if (xenogene is Gene_Metamorphosis xenogene_Met)
                {
                    if (xenogene_Met.Active) geneOptions.Add(xenogene_Met);
                }
            }
            bool considerEndogenes = geneOptions.Count <= 0;
            foreach (Gene endogene in pawn.genes.Endogenes)
            {
                if (endogene is Gene_Metamorphosis endogene_Met)
                {
                    if (considerEndogenes && endogene.Active) geneOptions.Add(endogene_Met);
                }
            }

            if (geneOptions.Count == 0)
            {
                Log.Error("[Cyanobot's Genes] Attempted to initiate metamorphosis for pawn: " + pawn + ", but could not find any Metamorphosis gene.");
                pawn.genes.RemoveGene(this);
                return;
            }

            Gene_Metamorphosis selected;
            geneOptions.TryRandomElementByWeight(g => CasteUtility.CasteCommonality(g.Xenotype), out selected);
            selected.chosenGene = true;

            foreach (Gene_Metamorphosis geneOption in geneOptions)
            {
                if (geneOption.chosenGene != true) geneOption.chosenGene = false;
            }

            //selected.Metamorphose();
        }

        public void Metamorphose()
        {
            CG_DefOf.CYB_Metamorphosis.SpawnMaintained(pawn, pawn.MapHeld);
            if (PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                string letterText = "CYB_LetterText_Metamorphosis".Translate().Formatted(pawn.Named("PAWN"), Xenotype.Named("XENOTYPE"));
                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("CYB_LetterLabel_Metamorphosis".Translate().Formatted(pawn.Named("PAWN")), letterText, LetterDefOf.NeutralEvent));

            }

            //remember current skin and hair colors
            GeneDef oldSkinColorOverrideDef = pawn.genes.GenesListForReading.Find(g => g.def.skinColorOverride != null && g.Active)?.def;
            GeneDef oldHairColorDef = pawn.genes.GenesListForReading.Find(g => g.def.endogeneCategory == EndogeneCategory.HairColor && g.Active)?.def;
            Color oldHairColor = pawn.story.HairColor;
            LogUtil.DebugLog("curSkinColorOverride: " + oldSkinColorOverrideDef + ", curHairColor: " + oldHairColorDef);

            //remove current xenotype
            //xenogenes or endogenes removed based on whether the metamorphosis gene is a xenogene or endogene

            List<Gene> toRemove;
            List<GeneDef> toAdd = Xenotype.AllGenes.ToList();

            bool xenogene = pawn.genes.Xenogenes.Contains(this);
            if (xenogene)
            {
                toRemove = pawn.genes.Xenogenes.ToList();
            }
            else
            {
                toRemove = pawn.genes.Endogenes.ToList();

                //don't remove their baseliner skin color gene
                toRemove.RemoveAll(g => g.def.endogeneCategory == EndogeneCategory.Melanin);

                //don't remove their hair color unless we're replacing it with something else
                List<Gene> curHairColors = toRemove.Where(g => g.def.endogeneCategory == EndogeneCategory.HairColor).ToList();
                List<GeneDef> potentialHairColors = Xenotype.AllGenes.Where(g => g.endogeneCategory == EndogeneCategory.HairColor).ToList();
                if (potentialHairColors.NullOrEmpty() && !curHairColors.NullOrEmpty())
                {
                    toRemove.RemoveAll(g => g.def.endogeneCategory == EndogeneCategory.HairColor && g.Active);
                }
            }

            foreach (GeneDef geneDef in Xenotype.AllGenes)
            {
                LogUtil.DebugLog("Looping Xenotype.AllGenes, geneDef: " + geneDef);
                GeneExtension_Bundle bundle = geneDef.GetModExtension<GeneExtension_Bundle>();
                if (bundle != null && bundle.Matches(toRemove.Select<Gene,GeneDef>(g => g.def)))
                {
                    toAdd.Remove(geneDef);
                    foreach(GeneDef bundleGeneDef in bundle.genes)
                    {
                        LogUtil.DebugLog("Looping bundle.genes, bundleGene: " + bundleGeneDef);
                        toRemove.RemoveAll(g => g.def == bundleGeneDef);
                    }
                }
                if (toRemove.ContainsAny(g => g.def == geneDef))
                {
                    toAdd.Remove(geneDef);
                    toRemove.RemoveAll(g => g.def == geneDef);
                }
            }

            foreach (Gene gene in toRemove)
            {
                LogUtil.DebugLog("Looping toRemove, gene: " + gene.def);
                pawn.genes.RemoveGene(gene);
            }

            //add new xenotype
            //again xenogenes or endogenes added based on whether metamorphosis was a xenogene or endogene

            pawn.genes.SetXenotypeDirect(Xenotype);
            pawn.genes.iconDef = null;
            foreach (GeneDef geneDef in toAdd)
            {
                LogUtil.DebugLog("Looping toAdd, geneDef: " + geneDef);
                pawn.genes.AddGene(geneDef, xenogene);
            }

            //if former skin and hair colors are still valid for the new geneset, enable them
            if (oldSkinColorOverrideDef != null)
            {
                List<Gene> newSkinColorOverrides = pawn.genes.Xenogenes.Where(g => g.def.skinColorOverride != null).ToList();
                if (newSkinColorOverrides.NullOrEmpty()) newSkinColorOverrides = pawn.genes.Endogenes.Where(g => g.def.skinColorOverride != null).ToList();
                if (!newSkinColorOverrides.NullOrEmpty())
                {
                    Gene oldSkinColorGene = newSkinColorOverrides.Find(g => g.def == oldSkinColorOverrideDef);
                    if (oldSkinColorGene != null)
                    {
                        LogUtil.DebugLog("Attempting to set skin color to " + oldSkinColorGene.def);
                        oldSkinColorGene.OverrideBy(null);
                        foreach (Gene gene in pawn.genes.GenesListForReading)
                        {
                            if (gene != oldSkinColorGene && gene.def.ConflictsWith(oldSkinColorGene.def))
                            {
                                gene.OverrideBy(oldSkinColorGene);
                            }
                        }
                    }
                }
            }
            if (oldHairColorDef != null)
            {
                List<Gene> newHairColors = pawn.genes.Xenogenes.Where(g => g.def.endogeneCategory == EndogeneCategory.HairColor).ToList();
                if (newHairColors.NullOrEmpty()) newHairColors = pawn.genes.Endogenes.Where(g => g.def.endogeneCategory == EndogeneCategory.HairColor).ToList();
                if (!newHairColors.NullOrEmpty())
                {
                    Gene oldHairColorGene = newHairColors.Find(g => g.def == oldHairColorDef);
                    if (oldHairColorGene != null)
                    {
                        LogUtil.DebugLog("Attempting to set hair color to " + oldHairColorGene.def);
                        oldHairColorGene.OverrideBy(null);
                        pawn.story.HairColor = oldHairColor;
                        foreach (Gene gene in pawn.genes.GenesListForReading)
                        {
                            if (gene != oldHairColorGene && gene.def.ConflictsWith(oldHairColorGene.def))
                            {
                                gene.OverrideBy(oldHairColorGene);
                            }
                        }
                    }
                }
            }

            m_EnsureCorrectSkinColorOverride.Invoke(pawn.genes, new object[] { });
        }
    }
}
