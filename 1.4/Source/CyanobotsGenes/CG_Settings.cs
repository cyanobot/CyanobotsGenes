using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CyanobotsGenes
{

    class CG_Settings : ModSettings
    {
        //default values
        public static bool unaffectedInTraitPool = true;
        public static bool moveVanillaGenes = true;
        public static bool changeMealStacking = true;

        public static float generationWeight_Biodrone = 1f;
        public static float generationWeight_Kitlin = 1f;

        //public static bool[] geneToggles = Enumerable.Repeat<bool>(true,CyanobotsGenes_Mod.geneList.Count).ToArray<bool>();

        //public static Dictionary<string,>

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref unaffectedInTraitPool, "unaffectedInTraitPool", unaffectedInTraitPool, true);
            Scribe_Values.Look(ref moveVanillaGenes, "moveVanillaGenes", moveVanillaGenes, true);
            Scribe_Values.Look(ref changeMealStacking, "changeMealStacking", changeMealStacking, true);
            Scribe_Values.Look(ref generationWeight_Biodrone, "generationWeight_Biodrone", generationWeight_Biodrone, true);
            Scribe_Values.Look(ref generationWeight_Kitlin, "generationWeight_Kitlin", generationWeight_Kitlin, true);
            /*
            foreach (KeyValuePair<int, string> kvp in CyanobotsGenes_Mod.geneDict)
                {
                Scribe_Values.Look(ref geneToggles[kvp.Key], kvp.Value + "_toggle", geneToggles[kvp.Key], true);
                }
            */
        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard l = new Listing_Standard(GameFont.Small)
            {
                ColumnWidth = rect.width
            };

            l.Begin(rect);

            l.CheckboxLabeled("CG_SettingLabel_UnaffectedInTraitPool".Translate(), ref unaffectedInTraitPool, "CG_SettingDesc_UnaffectedInTraitPool".Translate());
            l.CheckboxLabeled("CG_SettingLabel_MoveVanillaGenes".Translate(), ref moveVanillaGenes, "CG_SettingDesc_MoveVanillaGenes".Translate());
            l.CheckboxLabeled("CG_SettingLabel_ChangeMealStacking".Translate(), ref changeMealStacking, "CG_SettingDesc_ChangeMealStacking".Translate());

            l.Gap();

            l.Label("CG_SettingLabel_GenerationWeights".Translate());
            l.GapLine();

            l.Label(CG_DefOf.Biodrone.LabelCap + " : " + generationWeight_Biodrone.ToString("F2"));
            generationWeight_Biodrone = l.Slider(generationWeight_Biodrone, 0f, 100f);

            l.Label(CG_DefOf.Kitlin.LabelCap + " : " + generationWeight_Kitlin.ToString("F2"));
            generationWeight_Kitlin = l.Slider(generationWeight_Kitlin, 0f, 100f);

            /*
            l.Label("Individual gene toggles (requires restart)");

            foreach (KeyValuePair<int, string> kvp in CyanobotsGenes_Mod.geneDict)
            {
                string label = CyanobotsGenes_Mod.geneLabelDict[kvp.Key].CapitalizeFirst();
                l.CheckboxLabeled(label, ref geneToggles[kvp.Key], label);
            }
            */
            l.End();

            CG_Init.ApplySettingsToDefs();
        }
    }                                                   
}
