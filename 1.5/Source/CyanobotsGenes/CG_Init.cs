using HarmonyLib;
using RimWorld;
using Verse;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using static CyanobotsGenes.CG_Settings;
using static CyanobotsGenes.CG_Mod;
using UnityEngine;
using System;
using VanillaGenesExpanded;

namespace CyanobotsGenes
{
    [StaticConstructorOnStartup]
    static class CG_Init
    {
        public static Texture2D autoPsyphonIcon;

        static CG_Init()
        {
            autoPsyphonIcon = ContentFinder<Texture2D>.Get("UI/Icons/AutoPsyphon");

            alphaGenesLoaded = LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Alpha Genes");
            geologicalLandformsLoaded = LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Geological Landforms");
            forsakenNightLoaded = LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Alpha Biomes: Forsaken Night Unofficial Add-On");
            outlandGeneticsLoaded = LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Outland - Genetics");
            vreArchonLoaded = LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Vanilla Races Expanded - Archon");
            vreHighmateLoaded = LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Vanilla Races Expanded - Highmate");
            vreInsectorLoaded = LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Vanilla Races Expanded - Insector");
            vreSauridLoaded = LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Vanilla Races Expanded - Saurid");
            ebsgPsychicLoaded = LoadedModManager.RunningModsListForReading.Any(x => x.Name == "EBSG - Expanded Psychic Genes");
            ebsgAllInOneLoaded = LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Expanded Biotech Style Genes - All in One");

            if (forsakenNightLoaded) ThoughtWorker_LightSensitivity.lightlessBiomes.Add("AB_RockyCrags");
            if (geologicalLandformsLoaded)
            {
                ThoughtWorker_LightSensitivity.t_WorldTileInfo = AccessTools.TypeByName("GeologicalLandforms.WorldTileInfo");
                ThoughtWorker_LightSensitivity.m_WorldTileInfo_Get = AccessTools.Method(ThoughtWorker_LightSensitivity.t_WorldTileInfo, "Get", new Type[] { typeof(int), typeof(bool) });
                ThoughtWorker_LightSensitivity.p_BiomeVariants = AccessTools.Property(ThoughtWorker_LightSensitivity.t_WorldTileInfo, "BiomeVariants");
            }

            PopulateDefaults();
            ApplySettingsToDefs();
        }

        public static void PopulateDefaults()
        {
            TraitDef unaffectedDef = CG_DefOf.Unaffected;
            default_unaffectedCommonality = (float) unaffectedDef.GetType().GetField("commonality", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(unaffectedDef);

        }

        public static void ApplySettingsToDefs()
        {
            TraitDef unaffectedDef = CG_DefOf.Unaffected;
            float unaffectedCommonality;

            if (CG_Settings.unaffectedInTraitPool) unaffectedCommonality = default_unaffectedCommonality;
            else unaffectedCommonality = 0f;
            unaffectedDef.GetType().GetField("commonality", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(unaffectedDef, unaffectedCommonality);

            List<GeneDef> bundleGenes = DefDatabase<GeneDef>.AllDefsListForReading.Where(g => g.geneClass == typeof(Gene_Bundle)).ToList();
            //Log.Message("bundleGenes: " + bundleGenes.ToStringSafeEnumerable());
            HashSet<GeneDef> hidden_genes = StaticCollectionsClass.hidden_genes;
                //(HashSet<GeneDef>)f_hidden_genes.GetValue(null);
            foreach (GeneDef bundle in bundleGenes)
            {
                GeneExtension bundle_VEFGeneExtension = bundle.GetModExtension<GeneExtension>();
                    //.Find(e => e.GetType() == t_VEFGeneExtension);
                //Log.Message("bundle gene: " + bundle + ", bundle_VEFGeneExtension: " + bundle_VEFGeneExtension);
                if (bundle_VEFGeneExtension == null) continue;
                bundle_VEFGeneExtension.hideGene = !CG_Settings.bundleSkinHairColors;
                //f_hideGene.SetValue(bundle_VEFGeneExtension, !CG_Settings.bundleSkinHairColors);
                //Log.Message("Attempted setValue for bundle gene, current value: " + CG_Mod.f_hideGene.GetValue(bundle_VEFGeneExtension));

                if (CG_Settings.bundleSkinHairColors && hidden_genes.Contains(bundle))
                {
                    //Log.Message("Removing from hidden_genes");
                    hidden_genes.Remove(bundle);
                } 
                else if (!CG_Settings.bundleSkinHairColors && !hidden_genes.Contains(bundle))
                {
                    //Log.Message("Adding to hidden_genes");
                    hidden_genes.Add(bundle);
                }
                GeneExtension_Bundle bundleExtension = bundle.GetModExtension<GeneExtension_Bundle>();
                //Log.Message("bundleExtension: " + bundleExtension + ", genes: " + bundleExtension.genes.ToStringSafeEnumerable());
                if (bundleExtension == null || bundleExtension.genes.NullOrEmpty()) continue;
                foreach (GeneDef member in bundleExtension.genes)
                {
                    GeneExtension member_VEFGeneExtension = member.GetModExtension<GeneExtension>();
                        //.Find(e => e.GetType() == t_VEFGeneExtension);
                    //Log.Message("member gene: " + member + ", member_VEFGeneExtension: " + member_VEFGeneExtension);
                    if (member_VEFGeneExtension == null) continue;
                    member_VEFGeneExtension.hideGene = CG_Settings.bundleSkinHairColors;
                    //f_hideGene.SetValue(member_VEFGeneExtension, CG_Settings.bundleSkinHairColors);
                    //Log.Message("Attempted setValue for member gene, current value: " + CG_Mod.f_hideGene.GetValue(member_VEFGeneExtension));
                    
                    if (CG_Settings.bundleSkinHairColors && !hidden_genes.Contains(member))
                    {
                        hidden_genes.Add(member);
                    }
                    else if (!CG_Settings.bundleSkinHairColors && hidden_genes.Contains(member))
                    {
                        hidden_genes.Remove(member);
                    }
                }
            }

            CG_DefOf.Biodrone.factionlessGenerationWeight = generationWeight_Biodrone;
            CG_DefOf.CYB_Changeling.factionlessGenerationWeight = generationWeight_Changeling;
            CG_DefOf.CYB_Fairy.factionlessGenerationWeight = generationWeight_Fairy;
            if (outlandGeneticsLoaded || noOutlandGlimmer)
            {
                CG_DefOf.CYB_Glimmer.factionlessGenerationWeight = generationWeight_Glimmer;
                ReinstateXenotype(CG_DefOf.CYB_Glimmer);
            }
            else
            {
                RemoveXenotype(CG_DefOf.CYB_Glimmer);
            }
            CG_DefOf.Kitlin.factionlessGenerationWeight = generationWeight_Kitlin;
            if (vreArchonLoaded || ebsgAllInOneLoaded || ebsgPsychicLoaded)
            {
                CG_DefOf.CYB_Psycrux.factionlessGenerationWeight = generationWeight_Psycrux;
                ReinstateXenotype(CG_DefOf.CYB_Psycrux);
                ReinstateXenotype(CG_DefOf.CYB_Thrall);
            }
            else
            {
                RemoveXenotype(CG_DefOf.CYB_Psycrux);
                RemoveXenotype(CG_DefOf.CYB_Thrall);
            }
            CG_DefOf.CYB_Shulk.factionlessGenerationWeight = generationWeight_Shulk;
            CG_DefOf.CYB_Wist.factionlessGenerationWeight = generationWeight_Wist;

            
            foreach (string key in patchDict.Keys)
            {
                List<PatchWorker> patchList = patchDict[key];

                int i = 0;
                foreach (PatchWorker patchWorker in patchList)
                {
                    i++;
                    
                    if ((bool)typeof(CG_Settings).GetField(key, BindingFlags.Public | BindingFlags.Static).GetValue(null))
                    {
                        patchWorker.patchOn.Apply(patchWorker.xml);
                    }
                    else
                    {
                        patchWorker.patchOff.Apply(patchWorker.xml);
                    }
                    
                };
            };
        }

        public static void RemoveXenotype(XenotypeDef xenotype)
        {
            if (DefDatabase<XenotypeDef>.AllDefsListForReading.Contains(xenotype)) DefDatabase<XenotypeDef>.AllDefsListForReading.Remove(xenotype);
            if (!removedXenotypes.ContainsKey(xenotype)) removedXenotypes.Add(xenotype, new List<GeneDef>());
            foreach (GeneDef geneDef in DefDatabase<GeneDef>.AllDefsListForReading.ToList())
            {
                GeneExtension_Xenotype extension_Xenotype = geneDef.GetModExtension<GeneExtension_Xenotype>();
                if (extension_Xenotype != null && (extension_Xenotype.xenotype == xenotype))
                {
                    //Log.Message("Found extension_Xenotype for xenotpye " + xenotype + " on gene " + geneDef
                    //    + ". AllDefsList.Contains: " + DefDatabase<GeneDef>.AllDefsListForReading.Contains(geneDef));
                    if (DefDatabase<GeneDef>.AllDefsListForReading.Contains(geneDef)) DefDatabase<GeneDef>.AllDefsListForReading.Remove(geneDef);
                    //Log.Message("Post removal attempt, AllDefsList.Contains: " + DefDatabase<GeneDef>.AllDefsListForReading.Contains(geneDef));
                    removedXenotypes[xenotype].AddDistinct(geneDef);
                }
            }
            f_cachedGeneDefsInOrder.SetValue(null, null);
        }
        public static void ReinstateXenotype(XenotypeDef xenotype)
        {
            if (!removedXenotypes.ContainsKey(xenotype)) return;
            if (!DefDatabase<XenotypeDef>.AllDefsListForReading.Contains(xenotype)) DefDatabase<XenotypeDef>.AllDefsListForReading.Add(xenotype);
            if (!removedXenotypes[xenotype].NullOrEmpty())
            {
                foreach (GeneDef geneDef in removedXenotypes[xenotype])
                {
                    if (!DefDatabase<GeneDef>.AllDefsListForReading.Contains(geneDef)) DefDatabase<GeneDef>.AllDefsListForReading.Add(geneDef);
                }
            }
            removedXenotypes.Remove(xenotype);
            f_cachedGeneDefsInOrder.SetValue(null, null);
        }
    }
}
