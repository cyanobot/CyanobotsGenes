using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyanobotsGenes
{
    class CG_Mod : Mod
    {
        public static ModContentPack mcp;
        public static Harmony harmony;

        public static float default_unaffectedCommonality;

        public static Dictionary<string, List<PatchWorker>> patchDict = new Dictionary<string, List<PatchWorker>>();

        /*
        public static List<string> geneList = new List<string>() 
        {
            "Asocial",
            "Carnivore",
            "Hypercarnivore",
            "Herbivore",
            "StrictHerbivore",
            "EasilyBored",
            "RarelyBored",
            "NeverBored",
            "LightFur",
            "Tabby",
            "PackBond",
            "PreyDrive",
            "Psychopathic",
            "Stealthy"
        };
        //turns that list into an indexed dictionary
        public static Dictionary<int, string> geneDict = geneList.Select((s, i) => new { s, i }).ToDictionary(x => x.i, x => x.s);
        public static Dictionary<int, string> geneLabelDict = new Dictionary<int, string>();
        */

        public CG_Mod(ModContentPack mcp) : base(mcp)
        {
            CG_Mod.mcp = mcp;
            GetSettings<CG_Settings>();

            harmony = new Harmony("com.cyanobot.cyanobotsgenes");
            harmony.Patch(
                AccessTools.Method(typeof(RecipeDefGenerator), nameof(RecipeDefGenerator.ImpliedRecipeDefs)), 
                postfix: new HarmonyMethod(AccessTools.Method(typeof(ImpliedRecipeDefs_Patch),nameof(ImpliedRecipeDefs_Patch.Postfix)))
           );
        }

        public override string SettingsCategory()
        {
            return "Cyanobot's Genes";
        }

        public static GeneDef GetGeneDefFromName(string name)
        {
            return typeof(CG_DefOf).GetField(name, BindingFlags.Static | BindingFlags.Public).GetValue(null) as GeneDef;
        }

        public override void DoSettingsWindowContents(Rect inRect) => CG_Settings.DoSettingsWindowContents(inRect);

    }

    [StaticConstructorOnStartup]
    class CG_Init
    {
        static CG_Init()
        {
            PopulateDefaults();
            ApplySettingsToDefs();
        }

        public static void PopulateDefaults()
        {
            TraitDef unaffectedDef = CG_DefOf.Unaffected;
            CG_Mod.default_unaffectedCommonality = (float) unaffectedDef.GetType().GetField("commonality", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(unaffectedDef);

            /*
            foreach(KeyValuePair<int, string> kvp in CyanobotsGenes_Mod.geneDict)
            {
                string label = CyanobotsGenes_Mod.GetGeneDefFromName(kvp.Value).label;
                CyanobotsGenes_Mod.geneLabelDict.Add(kvp.Key, label);
            }
            */
        }

        public static void ApplySettingsToDefs()
        {
            TraitDef unaffectedDef = CG_DefOf.Unaffected;
            float unaffectedCommonality;

            if (CG_Settings.unaffectedInTraitPool) unaffectedCommonality = CG_Mod.default_unaffectedCommonality;
            else unaffectedCommonality = 0f;
            unaffectedDef.GetType().GetField("commonality", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(unaffectedDef, unaffectedCommonality);

            CG_DefOf.Biodrone.factionlessGenerationWeight = CG_Settings.generationWeight_Biodrone;
            CG_DefOf.Kitlin.factionlessGenerationWeight = CG_Settings.generationWeight_Kitlin;

            /*
            foreach (string key in CG_Mod.patchDict.Keys)
            {
                List<PatchWorker> patchList = CG_Mod.patchDict[key];

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
            */
        }
    }
}
