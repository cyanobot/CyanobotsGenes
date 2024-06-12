using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace CyanobotsGenes
{

    class PawnRenderNodeWorker_ByHeadTypeCategory : PawnRenderNodeWorker_FlipWhenCrawling
    {

        public static Dictionary<string, HeadTypeCategory> headTypeCategories = new Dictionary<string, HeadTypeCategory>
        {
            { "Male_AverageNormal", HeadTypeCategory.Male },
            { "Male_AveragePointy", HeadTypeCategory.Male },
            { "Male_AverageWide", HeadTypeCategory.Male },
            { "Male_NarrowNormal", HeadTypeCategory.MaleNarrow },
            { "Male_NarrowPointy", HeadTypeCategory.MaleNarrow },
            { "Male_NarrowWide", HeadTypeCategory.MaleNarrow },
            { "Female_AverageNormal", HeadTypeCategory.Female },
            { "Female_AveragePointy", HeadTypeCategory.Female },
            { "Female_AverageWide", HeadTypeCategory.Female },
            { "Female_NarrowNormal", HeadTypeCategory.FemaleNarrow },
            { "Female_NarrowPointy", HeadTypeCategory.FemaleNarrow },
            { "Female_NarrowWide", HeadTypeCategory.FemaleNarrow },
            { "Gaunt", HeadTypeCategory.Male },
            { "Male_HeavyJawNormal", HeadTypeCategory.HeavyJaw },
            { "Female_HeavyJawNormal", HeadTypeCategory.HeavyJaw },
            { "Furskin_Average1", HeadTypeCategory.Male },
            { "Furskin_Average2", HeadTypeCategory.Male },
            { "Furskin_Average3", HeadTypeCategory.Male },
            { "Furskin_Gaunt", HeadTypeCategory.Male },
            { "Furskin_Narrow1", HeadTypeCategory.Male },
            { "Furskin_Narrow2", HeadTypeCategory.Male },
            { "Furskin_Narrow3", HeadTypeCategory.Male },
            { "Furskin_Heavy1", HeadTypeCategory.HeavyJaw },
            { "Furskin_Heavy2", HeadTypeCategory.HeavyJaw },
            { "Furskin_Heavy3", HeadTypeCategory.HeavyJaw },
            { "Ghoul_Normal", HeadTypeCategory.Male },
            { "Ghoul_Heavy", HeadTypeCategory.Male },
            { "Ghoul_Narrow", HeadTypeCategory.Female },
            { "Ghoul_Wide", HeadTypeCategory.HeavyJaw },
            { "CultEscapee", HeadTypeCategory.Male },
            { "TimelessOne", HeadTypeCategory.Male },
            { "DarkScholar_Female", HeadTypeCategory.Male },
            { "DarkScholar_Male", HeadTypeCategory.Male },
            { "Leathery_Female", HeadTypeCategory.Male },
            { "Leathery_Male", HeadTypeCategory.Male },
            { "VRESaurids_ScaleSkin_Average", HeadTypeCategory.Male },
            { "VRESaurids_ScaleSkin_Narrow", HeadTypeCategory.Female },
            { "VRESaurids_ScaleSkin_Gaunt", HeadTypeCategory.Male },
            { "VRESaurids_ScaleSkin_Heavy", HeadTypeCategory.HeavyJaw },
            { "VRE_BarkAverage", HeadTypeCategory.Male },
            { "VRE_BarkAverage_Female", HeadTypeCategory.Male },
            { "VRE_BarkNarrow", HeadTypeCategory.Female },
            { "VRE_BarkNarrow_Female", HeadTypeCategory.Female },
            { "VRE_BarkGaunt", HeadTypeCategory.Male },
            { "VRE_BarkGaunt_Female", HeadTypeCategory.Male },
            { "VRE_BarkHeavy", HeadTypeCategory.HeavyJaw },
            { "VRE_BarkHeavy_Female", HeadTypeCategory.HeavyJaw },
            { "VRE_Leatherskin_Average_Normal", HeadTypeCategory.Male },
            { "VRE_LeatherskinFemale_Average_Normal", HeadTypeCategory.Female },
            { "VRE_Leatherskin_Narrow_Normal", HeadTypeCategory.Female },
            { "VRE_LeatherskinFemale_Narrow_Normal", HeadTypeCategory.Female },
            { "VRE_Leatherskin_Gaunt_Normal", HeadTypeCategory.Male },
            { "VRE_LeatherskinFemale_Gaunt_Normal", HeadTypeCategory.Male },
            { "VRE_Leatherskin_Wide_Normal", HeadTypeCategory.HeavyJaw },
            { "VRE_LeatherskinFemale_Wide_Normal", HeadTypeCategory.HeavyJaw },
            { "AG_SlugFace_Narrow", HeadTypeCategory.MaleNarrow },
            { "AG_SlugFace_Normal", HeadTypeCategory.Male },
            { "AG_SlugFace_Wide", HeadTypeCategory.FemaleNarrow },
            { "AG_Male_Eyeless_Average_Normal", HeadTypeCategory.Male },
            { "AG_Male_Eyeless_Average_Pointy", HeadTypeCategory.Male },
            { "AG_Male_Eyeless_Average_Wide", HeadTypeCategory.Male },
            { "AG_Male_Eyeless_Narrow_Normal", HeadTypeCategory.MaleNarrow },
            { "AG_Male_Eyeless_Narrow_Pointy", HeadTypeCategory.MaleNarrow },
            { "AG_Male_Eyeless_Narrow_Wide", HeadTypeCategory.MaleNarrow },
            { "AG_Female_Eyeless_Average_Normal", HeadTypeCategory.Female },
            { "AG_Female_Eyeless_Average_Pointy", HeadTypeCategory.Female },
            { "AG_Female_Eyeless_Average_Wide", HeadTypeCategory.Female },
            { "AG_Female_Eyeless_Narrow_Normal", HeadTypeCategory.FemaleNarrow },
            { "AG_Female_Eyeless_Narrow_Pointy", HeadTypeCategory.FemaleNarrow },
            { "AG_Female_Eyeless_Narrow_Wide", HeadTypeCategory.FemaleNarrow },
            { "AG_TeratoHead_Male_Normal", HeadTypeCategory.Male },
            { "AG_TeratoHead_Male_Melting", HeadTypeCategory.Male },
            { "AG_TeratoHead_Male_Narrow", HeadTypeCategory.Female },
            { "AG_TeratoHead_Female_Normal", HeadTypeCategory.Male },
            { "AG_TeratoHead_Female_Melting", HeadTypeCategory.Male },
            { "AG_TeratoHead_Female_Narrow", HeadTypeCategory.Female },
            { "AG_Male_Silver_Average_Normal", HeadTypeCategory.Male },
            { "AG_Male_Silver_Average_Pointy", HeadTypeCategory.Male },
            { "AG_Male_Silver_Average_Wide", HeadTypeCategory.Male },
            { "AG_Male_Silver_Narrow_Normal", HeadTypeCategory.MaleNarrow },
            { "AG_Male_Silver_Narrow_Pointy", HeadTypeCategory.MaleNarrow },
            { "AG_Male_Silver_Narrow_Wide", HeadTypeCategory.MaleNarrow },
            { "AG_Female_Silver_Average_Normal", HeadTypeCategory.Female },
            { "AG_Female_Silver_Average_Pointy", HeadTypeCategory.Female },
            { "AG_Female_Silver_Average_Wide", HeadTypeCategory.Female },
            { "AG_Female_Silver_Narrow_Normal", HeadTypeCategory.FemaleNarrow },
            { "AG_Female_Silver_Narrow_Pointy", HeadTypeCategory.FemaleNarrow },
            { "AG_Female_Silver_Narrow_Wide", HeadTypeCategory.FemaleNarrow },
            { "AG_Male_Golden_Average_Normal", HeadTypeCategory.Male },
            { "AG_Male_Golden_Average_Pointy", HeadTypeCategory.Male },
            { "AG_Male_Golden_Average_Wide", HeadTypeCategory.Male },
            { "AG_Male_Golden_Narrow_Normal", HeadTypeCategory.MaleNarrow },
            { "AG_Male_Golden_Narrow_Pointy", HeadTypeCategory.MaleNarrow },
            { "AG_Male_Golden_Narrow_Wide", HeadTypeCategory.MaleNarrow },
            { "AG_Female_Golden_Average_Normal", HeadTypeCategory.Female },
            { "AG_Female_Golden_Average_Pointy", HeadTypeCategory.Female },
            { "AG_Female_Golden_Average_Wide", HeadTypeCategory.Female },
            { "AG_Female_Golden_Narrow_Normal", HeadTypeCategory.FemaleNarrow },
            { "AG_Female_Golden_Narrow_Pointy", HeadTypeCategory.FemaleNarrow },
            { "AG_Female_Golden_Narrow_Wide", HeadTypeCategory.FemaleNarrow },
            { "AG_Male_TrueBlack_Average_Normal", HeadTypeCategory.Male },
            { "AG_Male_TrueBlack_Average_Pointy", HeadTypeCategory.Male },
            { "AG_Male_TrueBlack_Average_Wide", HeadTypeCategory.Male },
            { "AG_Male_TrueBlack_Narrow_Normal", HeadTypeCategory.MaleNarrow },
            { "AG_Male_TrueBlack_Narrow_Pointy", HeadTypeCategory.MaleNarrow },
            { "AG_Male_TrueBlack_Narrow_Wide", HeadTypeCategory.MaleNarrow },
            { "AG_Female_TrueBlack_Average_Normal", HeadTypeCategory.Female },
            { "AG_Female_TrueBlack_Average_Pointy", HeadTypeCategory.Female },
            { "AG_Female_TrueBlack_Average_Wide", HeadTypeCategory.Female },
            { "AG_Female_TrueBlack_Narrow_Normal", HeadTypeCategory.FemaleNarrow },
            { "AG_Female_TrueBlack_Narrow_Pointy", HeadTypeCategory.FemaleNarrow },
            { "AG_Female_TrueBlack_Narrow_Wide", HeadTypeCategory.FemaleNarrow },
            { "AG_Female_VeryGaunt", HeadTypeCategory.Male },
            { "AG_Male_VeryGaunt", HeadTypeCategory.Male },
            { "Outland_DestinyMarks_MaleNormal", HeadTypeCategory.Male },
            { "Outland_DestinyMarks_MalePointy", HeadTypeCategory.Male },
            { "Outland_DestinyMarks_MaleWide", HeadTypeCategory.Male },
            { "Outland_DestinyMarks_FemaleNormal", HeadTypeCategory.Female },
            { "Outland_DestinyMarks_FemalePointy", HeadTypeCategory.Female },
            { "Outland_DestinyMarks_FemaleWide", HeadTypeCategory.Female },
            { "Outland_Male_Goblinoid", HeadTypeCategory.Male },
            { "Outland_Female_Goblinoid", HeadTypeCategory.Male },
            { "Outland_None_Goblinoid", HeadTypeCategory.Male },
            { "Outland_Insect", HeadTypeCategory.Male },
            { "Outland_Termite", HeadTypeCategory.Male },
            { "Outland_Male_Moth", HeadTypeCategory.Male },
            { "Outland_Female_Moth", HeadTypeCategory.Male },
            { "Outland_ScaleSkin", HeadTypeCategory.Male },
            { "Outland_Male_Svelte", HeadTypeCategory.Male },
            { "Outland_Female_Svelte", HeadTypeCategory.Male },

        };

        public HeadTypeCategory CategoryFromHeadType(HeadTypeDef headType)
        {
            return headTypeCategories.ContainsKey(headType.defName) ? headTypeCategories[headType.defName] : HeadTypeCategory.Undefined;
        }

        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            //Log.Message("CanDrawNow - base: " + base.CanDrawNow(node, parms)
            //    + ", pawn.def: " + parms.pawn.def
            //    + ", headType: " + parms.pawn.story?.headType
            //    + ", ContainsKey: " + headTypeCategories.ContainsKey(parms.pawn.story.headType.defName));
            if (!base.CanDrawNow(node, parms)) return false;
            if (parms.pawn.def != ThingDefOf.Human
                || parms.pawn.story?.headType == null
                || !headTypeCategories.ContainsKey(parms.pawn.story.headType.defName))
            {
                //Log.Message("CanDrawNow returning false");
                return false;
            }
            return true;
        }

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            Vector3 scale = base.ScaleFor(node, parms);
            if (!(node is PawnRenderNode_ByHeadTypeCategory node_byHeadType)) return scale;

            HeadTypeDef headType = parms.pawn.story?.headType;
            if (headType == null || !headTypeCategories.ContainsKey(headType.defName))
                return scale;

            HeadTypeCategory category = CategoryFromHeadType(headType);
            if (category == HeadTypeCategory.Undefined) return scale;

            HeadTypeCategoryData data = node_byHeadType.DataFor(category, parms.facing);
            if (data == null) return scale;

            scale = scale.MultipliedBy(data.scale);
            return scale;
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            //Log.Message("OffsetFor - pawn: " + parms.pawn + ", node: " + node);
            Vector3 offset = base.OffsetFor(node, parms, out pivot);
            if (!(node is PawnRenderNode_ByHeadTypeCategory node_byHeadType)) return offset;

            HeadTypeDef headType = parms.pawn.story?.headType;
            if (headType == null || !headTypeCategories.ContainsKey(headType.defName))
                return offset;

            HeadTypeCategory category = CategoryFromHeadType(headType);
            //Log.Message("headType: " + headType + ", cat: " + category);
            if (category == HeadTypeCategory.Undefined) return offset;

            HeadTypeCategoryData data = node_byHeadType.DataFor(category, parms.facing);
            //Log.Message("data: " + data);
            if (data == null) return offset;

            offset += data.offset;

            //Log.Message("OffsetFor - data.offset: (" 
             //   + data.offset.x + "," + data.offset.y + "," + data.offset.z + ")");

            return offset;
        }
    }
}
