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

            l.CheckboxLabeled("Add Unaffected trait to trait pool", ref unaffectedInTraitPool, "Whether the Unaffected trait should generate randomly on pawns, independently of the Asocial gene.");
            l.CheckboxLabeled("Change vanilla gene categories (requires restart)", ref moveVanillaGenes, "Moves some vanilla genes out of Miscellaneous to group them with related genes from this mod, eg Robust Digestion into the Diet category.");
            l.CheckboxLabeled("Picky meal stacking (requires restart)", ref changeMealStacking, "Tighter restrictions on meal stacking by ingredient. If you have no herbivorous/carnivorous pawns and would prefer the vanilla logic, turn this off. NOTE: failure to restart after changing will cause errors and invisble meals.");

            l.Gap();

            l.Label("Xenotype Generation Weights (factionlessGenerationWeight)");
            l.GapLine();

            l.Label("Biodrone : " + generationWeight_Biodrone.ToString("F2"));
            generationWeight_Biodrone = l.Slider(generationWeight_Biodrone, 0f, 100f);

            l.Label("Kitlin : " + generationWeight_Kitlin.ToString("F2"));
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
