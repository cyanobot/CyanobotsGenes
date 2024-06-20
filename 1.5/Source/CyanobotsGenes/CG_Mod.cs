using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyanobotsGenes
{
    class CG_Mod : Mod
    {
        public static ModContentPack mcp;
        public static Harmony harmony;

        public static bool geologicalLandformsLoaded;
        public static bool forsakenNightLoaded;
        public static bool outlandGeneticsLoaded;
        public static bool vreArchonLoaded;
        public static bool ebsgPsychicLoaded;
        public static bool ebsgAllInOneLoaded;

        public static float default_unaffectedCommonality;

        public static Dictionary<string, List<PatchWorker>> patchDict = new Dictionary<string, List<PatchWorker>>();
        public static Dictionary<XenotypeDef, List<GeneDef>> removedXenotypes = new Dictionary<XenotypeDef, List<GeneDef>>();

        public static Type t_VEFGeneExtension = AccessTools.TypeByName("VanillaGenesExpanded.GeneExtension");
        public static Type t_VEFStaticCollectionsClass = AccessTools.TypeByName("VanillaGenesExpanded.StaticCollectionsClass");
        public static FieldInfo f_hideGene = AccessTools.Field(t_VEFGeneExtension, "hideGene");
        public static FieldInfo f_hidden_genes = AccessTools.Field(t_VEFStaticCollectionsClass, "hidden_genes");
        public static FieldInfo f_cachedGeneDefsInOrder = AccessTools.Field(typeof(GeneUtility), "cachedGeneDefsInOrder");

        public CG_Mod(ModContentPack mcp) : base(mcp)
        {
            CG_Mod.mcp = mcp;
            GetSettings<CG_Settings>();

            harmony = new Harmony("com.cyanobot.cyanobotsgenes");
            harmony.Patch(
                AccessTools.Method(typeof(RecipeDefGenerator), nameof(RecipeDefGenerator.ImpliedRecipeDefs)), 
                postfix: new HarmonyMethod(AccessTools.Method(typeof(ImpliedRecipeDefs_Patch),nameof(ImpliedRecipeDefs_Patch.Postfix)))
           );
            harmony.Patch(
                AccessTools.Method(typeof(GeneDefGenerator), "ImpliedGeneDefs"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(ImpliedGeneDefs_Patch), nameof(ImpliedGeneDefs_Patch.Postfix)))
           );
        }

        public override string SettingsCategory()
        {
            return "Cyanobot's Genes";
        }

        public override void DoSettingsWindowContents(Rect inRect) => CG_Settings.DoSettingsWindowContents(inRect);

    }
}
