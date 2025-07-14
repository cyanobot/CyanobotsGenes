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
using static CyanobotsGenes.CG_Mod;
using Verse.Noise;

namespace CyanobotsGenes
{

    public class CG_Settings : ModSettings
    {
        //default values
        public static bool unaffectedInTraitPool = true;
        public static bool moveVanillaGenes = true;
        public static bool changeMealStacking = true;
        public static bool bundleSkinHairColors = true;

        public static bool offspringAffects_AsexualFission = false;
        public static bool offspringAffects_AG_ParasiticEndogenes = false;
        public static bool offspringAffects_AG_ParasiticXenogenes = false;

#if RW_1_5
        public static bool noOutlandGlimmer = false;

        public static float generationWeight_Biodrone = 1f;
        public static float generationWeight_Changeling = 1f;
        public static float generationWeight_Fairy = 1f;
        public static float generationWeight_Glimmer = 1f;
        public static float generationWeight_Kitlin = 1f;
        public static float generationWeight_Psycrux = 0f;
        public static float generationWeight_Shulk = 1f;
        public static float generationWeight_Wist = 1f;
#endif

        private static Vector2 scrollPosition = new Vector2(0f, 0f);

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref unaffectedInTraitPool, "unaffectedInTraitPool", unaffectedInTraitPool, true);
            Scribe_Values.Look(ref moveVanillaGenes, "moveVanillaGenes", moveVanillaGenes, true);
            Scribe_Values.Look(ref changeMealStacking, "changeMealStacking", changeMealStacking, true);
            Scribe_Values.Look(ref bundleSkinHairColors, "bundleSkinHairColors", bundleSkinHairColors, true);

            Scribe_Values.Look(ref offspringAffects_AsexualFission, "offspringAffects_AsexualFission", offspringAffects_AsexualFission, true);
            Scribe_Values.Look(ref offspringAffects_AG_ParasiticEndogenes, "offspringAffects_AG_ParasiticEndogenes", offspringAffects_AG_ParasiticEndogenes, true);
            Scribe_Values.Look(ref offspringAffects_AG_ParasiticXenogenes, "offspringAffects_AG_ParasiticXenogenes", offspringAffects_AG_ParasiticXenogenes, true);

#if RW_1_5
            Scribe_Values.Look(ref noOutlandGlimmer, "noOutlandGlimmer", noOutlandGlimmer, true);

            Scribe_Values.Look(ref generationWeight_Biodrone, "generationWeight_Biodrone", generationWeight_Biodrone, true);
            Scribe_Values.Look(ref generationWeight_Changeling, "generationWeight_Changeling", generationWeight_Changeling, true);
            Scribe_Values.Look(ref generationWeight_Fairy, "generationWeight_Fairy", generationWeight_Fairy, true);
            Scribe_Values.Look(ref generationWeight_Glimmer, "generationWeight_Glimmer", generationWeight_Glimmer, true);
            Scribe_Values.Look(ref generationWeight_Kitlin, "generationWeight_Kitlin", generationWeight_Kitlin, true);
            Scribe_Values.Look(ref generationWeight_Psycrux, "generationWeight_Psycrux", generationWeight_Psycrux, true);
            Scribe_Values.Look(ref generationWeight_Shulk, "generationWeight_Shulk", generationWeight_Shulk, true);
            Scribe_Values.Look(ref generationWeight_Wist, "generationWeight_Wist", generationWeight_Wist, true);
#endif

        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            float totalContentHeight = 1000f;
            float scrollBarWidth = 18f;

            Rect contentRect = new Rect(rect);
            Rect viewRect = new Rect(rect);
            viewRect.height = totalContentHeight;

            Widgets.DrawHighlight(contentRect);
            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);


            Listing_Standard l = new Listing_Standard(GameFont.Small)
            {
                ColumnWidth = contentRect.width - scrollBarWidth
            };

            l.Begin(viewRect);

            l.CheckboxLabeled("CYB_SettingLabel_UnaffectedInTraitPool".Translate(), ref unaffectedInTraitPool, "CYB_SettingDesc_UnaffectedInTraitPool".Translate());
            l.CheckboxLabeled("CYB_SettingLabel_MoveVanillaGenes".Translate(), ref moveVanillaGenes, "CYB_SettingDesc_MoveVanillaGenes".Translate());
            l.CheckboxLabeled("CYB_SettingLabel_ChangeMealStacking".Translate(), ref changeMealStacking, "CYB_SettingDesc_ChangeMealStacking".Translate());
            l.CheckboxLabeled("CYB_SettingLabel_BundleSkinHairColors".Translate(), ref bundleSkinHairColors, "CYB_SettingDesc_BundleSkinHairColors".Translate());
#if RW_1_5
            l.CheckboxLabeled("CYB_SettingLabel_NoOutlandGlimmer".Translate(), ref noOutlandGlimmer, "CYB_SettingDesc_NoOutlandGlimmer".Translate());
#endif

            l.Gap();
            l.Gap();

            l.Label("CYB_SettingsHeader_OffspringAffects".Translate(), tooltip: "CYB_SettingsHeaderDesc_OffspringAffects".Translate());

            l.GapLine();

            l.CheckboxLabeled("CYB_SettingLabel_OffspringAffects_AsexualFission".Translate(), ref offspringAffects_AsexualFission, "CYB_SettingDesc_OffspringAffects_AsexualFission".Translate());
            l.CheckboxLabeled("CYB_SettingLabel_OffspringAffects_AG_ParasiticEndogenes".Translate(), ref offspringAffects_AG_ParasiticEndogenes, "CYB_SettingDesc_OffspringAffects_AG_ParasiticEndogenes".Translate());
            l.CheckboxLabeled("CYB_SettingLabel_OffspringAffects_AG_ParasiticXenogenes".Translate(), ref offspringAffects_AG_ParasiticXenogenes, "CYB_SettingDesc_OffspringAffects_AG_ParasiticXenogenes".Translate());

#if RW_1_5
            l.Gap();
            l.Gap();

            l.Label("CYB_SettingLabel_GenerationWeights".Translate());
            l.GapLine();

            l.Label(CG_DefOf.Biodrone.LabelCap + " : " + generationWeight_Biodrone.ToString("F2"));
            generationWeight_Biodrone = l.Slider(generationWeight_Biodrone, 0f, 100f);

            l.Label(CG_DefOf.CYB_Changeling.LabelCap + " : " + generationWeight_Changeling.ToString("F2"));
            generationWeight_Changeling = l.Slider(generationWeight_Changeling, 0f, 100f);

            l.Label(CG_DefOf.CYB_Fairy.LabelCap + " : " + generationWeight_Fairy.ToString("F2"));
            generationWeight_Fairy = l.Slider(generationWeight_Fairy, 0f, 100f);

            if (outlandGeneticsLoaded || noOutlandGlimmer)
            {
                l.Label(CG_DefOf.CYB_Glimmer.LabelCap + " : " + generationWeight_Glimmer.ToString("F2"));
                generationWeight_Glimmer = l.Slider(generationWeight_Glimmer, 0f, 100f);
            }

            l.Label(CG_DefOf.Kitlin.LabelCap + " : " + generationWeight_Kitlin.ToString("F2"));
            generationWeight_Kitlin = l.Slider(generationWeight_Kitlin, 0f, 100f);

            if (vreArchonLoaded)
            {
                l.Label(CG_DefOf.CYB_Psycrux.LabelCap + " : " + generationWeight_Psycrux.ToString("F2"));
                generationWeight_Psycrux = l.Slider(generationWeight_Psycrux, 0f, 100f);
            }

            l.Label(CG_DefOf.CYB_Shulk.LabelCap + " : " + generationWeight_Shulk.ToString("F2"));
            generationWeight_Shulk = l.Slider(generationWeight_Shulk, 0f, 100f);

            l.Label(CG_DefOf.CYB_Wist.LabelCap + " : " + generationWeight_Wist.ToString("F2"));
            generationWeight_Wist = l.Slider(generationWeight_Wist, 0f, 100f);
#endif
            l.End();

            Widgets.EndScrollView();

            CG_Init.ApplySettingsToDefs();
        }
    }                                                   
}
